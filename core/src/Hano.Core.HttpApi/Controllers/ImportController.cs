using Hano.Core.Import;
using Hano.Core.Import.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bamboo.Shared.Common;
using System.IO;
using System.Threading;

namespace Hano.Core.HttpApi.Controllers;

public class NoTimeoutStream : Stream
{
    private readonly Stream _inner;

    public NoTimeoutStream(Stream inner)
    {
        _inner = inner;
    }

    public override bool CanRead => _inner.CanRead;
    public override bool CanSeek => _inner.CanSeek;
    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _inner.Length;
    public override long Position { get => _inner.Position; set => _inner.Position = value; }

    public override int ReadTimeout => Timeout.Infinite; // 🔥 FIX
    public override int WriteTimeout => Timeout.Infinite;


    public override int Read(byte[] buffer, int offset, int count)
        => _inner.Read(buffer, offset, count);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _inner.ReadAsync(buffer, offset, count, cancellationToken);

    public override void Flush() => _inner.Flush();
    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
    public override void SetLength(long value) => _inner.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
}

/// <summary>
/// Bulk import of organisational master data, customer outlets, and SKUs.
/// All endpoints require role: dms_admin.
///
/// Recommended import order:
///   1. POST /api/v1/import/master-data  (creates regions, users, NPP tenants)
///   2. POST /api/v1/import/customers    (creates outlets linked to regions)
///   3. POST /api/v1/import/skus         (creates host-level SKU catalogue)
/// </summary>
[Route("api/v1/import")]
[ApiController]
[Authorize(Roles = "admin")]
public class ImportController(IImportAppService importAppService) : HanoCoreController
{
    /// <summary>
    /// Import "Danh sách vùng, NPP, GSBH, NVBH, ADMIN" Excel file.
    /// Creates: OrganizationUnits, Users (ASM/GSBH/NVBH/admin), Tenants, Distributors.
    /// </summary>
    [HttpPost("master-data")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [Consumes("multipart/form-data")]
    public async Task<ImportMasterDataResult> ImportMasterData(
        IFormFile file,
        [FromForm] ExcelReaderType readerType = ExcelReaderType.MiniExcel,
        [FromForm] bool dryRun = false)
    {
        await using var stream = file.OpenReadStream();
        await using var safeStream = new NoTimeoutStream(stream);

        // await using var memoryStream = new MemoryStream();

        // await stream.CopyToAsync(memoryStream);
        // memoryStream.Position = 0;

        return await importAppService.ImportMasterDataAsync(safeStream, new ImportMasterDataInput
        {
            ReaderType = readerType,
            DryRun = dryRun,
        });
    }

    /// <summary>
    /// Import "Danh sách khách hàng" Excel file.
    /// Creates: Outlets (host-level, assigned to region OrganizationUnits).
    /// Run AFTER master-data import.
    /// </summary>
    [HttpPost("customers")]
    [RequestSizeLimit(104_857_600)] // 100 MB (≈18k rows)
    [Consumes("multipart/form-data")]
    public async Task<ImportCustomersResult> ImportCustomers(
        IFormFile file,
        [FromForm] ExcelReaderType readerType = ExcelReaderType.MiniExcel,
        [FromForm] bool dryRun = false)
    {
        await using var stream = file.OpenReadStream();
        await using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return await importAppService.ImportCustomersAsync(memoryStream, new ImportCustomersInput
        {
            ReaderType = readerType,
            DryRun = dryRun,
        });
    }

    /// <summary>
    /// Import SKU catalogue from "SKU.xlsx".
    /// Creates: host-level SKUs (TenantId=null). Idempotent by SKU_GOC code.
    /// </summary>
    [HttpPost("skus")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    [Consumes("multipart/form-data")]
    public async Task<ImportSkusResult> ImportSkus(
        IFormFile file,
        [FromForm] ExcelReaderType readerType = ExcelReaderType.MiniExcel,
        [FromForm] bool dryRun = false)
    {
        await using var stream = file.OpenReadStream();
        await using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return await importAppService.ImportSkusAsync(memoryStream, new ImportSkusInput
        {
            ReaderType = readerType,
            DryRun = dryRun,
        });
    }
}
