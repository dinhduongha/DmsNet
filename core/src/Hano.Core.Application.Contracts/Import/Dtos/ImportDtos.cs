using System.Collections.Generic;

namespace Hano.Core.Import.Dtos;

public enum ExcelReaderType
{
    MiniExcel,
    EPPlus,
    NPOI
}

// ─── Input DTOs ──────────────────────────────────────────────────────────────

public class ImportMasterDataInput
{
    /// <summary>Excel library to use for reading. Default: MiniExcel.</summary>
    public ExcelReaderType ReaderType { get; set; } = ExcelReaderType.MiniExcel;

    /// <summary>Validate without persisting to the database.</summary>
    public bool DryRun { get; set; }
}

public class ImportCustomersInput
{
    /// <summary>Excel library to use for reading. Default: MiniExcel.</summary>
    public ExcelReaderType ReaderType { get; set; } = ExcelReaderType.MiniExcel;

    /// <summary>Validate without persisting to the database.</summary>
    public bool DryRun { get; set; }
}

public class ImportSkusInput
{
    /// <summary>Excel library to use for reading. Default: MiniExcel.</summary>
    public ExcelReaderType ReaderType { get; set; } = ExcelReaderType.MiniExcel;

    /// <summary>Validate without persisting to the database.</summary>
    public bool DryRun { get; set; }
}

// ─── Master Data Result DTOs ──────────────────────────────────────────────────

/// <summary>
/// Result of Step 1: creating ABP system entities
/// (AbpUsers, AbpOrganizationUnits, AbpUserOrganizationUnits, AbpTenants).
/// </summary>
public class ImportAbpEntitiesResult
{
    public int UsersCreated { get; set; }
    public int UsersSkipped { get; set; }
    public int OusCreated { get; set; }
    public int OusSkipped { get; set; }
    public int TenantsCreated { get; set; }
    public int TenantsSkipped { get; set; }
    public bool IsDryRun { get; set; }

    /// <summary>
    /// Created user credentials. Distribute securely — passwords are returned only once.
    /// </summary>
    public List<ImportedUserRecord> CreatedUsers { get; set; } = [];
    public List<ImportErrorDto> Errors { get; set; } = [];
}

/// <summary>
/// Result of Step 2: creating domain records
/// (regions, teams, distributors tables).
/// Reads ABP entities from DB — does NOT depend on Step 1 result object.
/// </summary>
public class ImportDomainRecordsResult
{
    public int RegionsCreated { get; set; }
    public int RegionsSkipped { get; set; }
    public int TeamsCreated { get; set; }
    public int TeamsSkipped { get; set; }
    public int DistributorsCreated { get; set; }
    public int DistributorsSkipped { get; set; }
    public bool IsDryRun { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = [];
}

/// <summary>
/// Combined HTTP response for POST /api/v1/import/master-data.
/// The controller calls both steps sequentially and merges them here.
/// </summary>
public class ImportMasterDataResult
{
    public ImportAbpEntitiesResult AbpEntities { get; set; } = new();
    public ImportDomainRecordsResult DomainRecords { get; set; } = new();
    public bool IsDryRun { get; set; }
}

// ─── Other Result DTOs ────────────────────────────────────────────────────────

public class ImportCustomersResult
{
    public int OutletsCreated { get; set; }
    public int OutletsSkipped { get; set; }
    public bool IsDryRun { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = [];
}

public class ImportSkusResult
{
    public int SkusCreated { get; set; }
    public int SkusSkipped { get; set; }
    public bool IsDryRun { get; set; }
    public List<ImportErrorDto> Errors { get; set; } = [];
}

// ─── Shared ───────────────────────────────────────────────────────────────────

public class ImportedUserRecord
{
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Plain-text password — only returned at creation time.
    /// Format: {FamilyCased}{GivenCased}$$${index}.
    /// </summary>
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}

public class ImportErrorDto
{
    public string Sheet { get; set; } = string.Empty;
    public int RowNumber { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>Fatal errors abort the entire import step.</summary>
    public bool IsFatal { get; set; }
}
