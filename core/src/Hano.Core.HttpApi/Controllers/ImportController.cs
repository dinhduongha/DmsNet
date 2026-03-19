using Hano.Core.Import;
using Hano.Core.Import.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Bamboo.Shared.Common;

namespace Hano.Core.HttpApi.Controllers;

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
        return await importAppService.ImportMasterDataAsync(stream, new ImportMasterDataInput
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
        return await importAppService.ImportCustomersAsync(stream, new ImportCustomersInput
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
        return await importAppService.ImportSkusAsync(stream, new ImportSkusInput
        {
            ReaderType = readerType,
            DryRun = dryRun,
        });
    }
}
