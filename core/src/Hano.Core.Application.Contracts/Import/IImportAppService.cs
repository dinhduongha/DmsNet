using System.IO;
using System.Threading.Tasks;
using Hano.Core.Import.Dtos;

namespace Hano.Core.Import;

public interface IImportAppService
{
    /// <summary>
    /// Step 1 — Import ABP system entities from the master-data Excel file.
    /// Creates: AbpUsers (all roles), AbpOrganizationUnits (regions + teams),
    ///          AbpUserOrganizationUnits, AbpTenants (one per NPP).
    /// Must be called before ImportDomainRecordsAsync.
    /// </summary>
    Task<ImportAbpEntitiesResult> ImportAbpEntitiesAsync(Stream fileStream, ImportMasterDataInput input);

    /// <summary>
    /// Step 2 — Import domain records from the master-data Excel file.
    /// Creates: regions table (Organization), teams table (Team), distributors table.
    /// Fetches ABP entities (OUs, users, tenants) fresh from DB — independent of Step 1 result.
    /// Must be called after ImportAbpEntitiesAsync.
    /// </summary>
    Task<ImportDomainRecordsResult> ImportDomainRecordsAsync(Stream fileStream, ImportMasterDataInput input);

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
