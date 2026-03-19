using System.IO;
using System.Threading.Tasks;
using Hano.Core.Import.Dtos;

namespace Hano.Core.Import;

public interface IImportAppService
{
    /// <summary>
    /// Import master data from "Danh sách vùng, NPP, GSBH, NVBH, ADMIN" Excel file.
    /// Creates: OrganizationUnits (regions), Users with roles, Tenants + Distributors (NPP).
    /// Must be run BEFORE ImportCustomersAsync.
    /// </summary>
    Task<ImportMasterDataResult> ImportMasterDataAsync(Stream fileStream, ImportMasterDataInput input);

    /// <summary>
    /// Import customer list from "Danh sách khách hàng" Excel file.
    /// Creates: Outlets (TenantId=null, assigned to region OrganizationUnit).
    /// Requires regions to be imported first.
    /// </summary>
    Task<ImportCustomersResult> ImportCustomersAsync(Stream fileStream, ImportCustomersInput input);

    /// <summary>
    /// Import SKU list from "SKU.xlsx" file.
    /// Creates: host-level SKUs (TenantId=null). Idempotent by SKU_GOC code.
    /// </summary>
    Task<ImportSkusResult> ImportSkusAsync(Stream fileStream, ImportSkusInput input);
}
