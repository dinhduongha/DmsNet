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

// ─── Result DTOs ─────────────────────────────────────────────────────────────

public class ImportMasterDataResult
{
    public int RegionsCreated { get; set; }
    public int RegionsSkipped { get; set; }
    public int UsersCreated { get; set; }
    public int UsersSkipped { get; set; }
    public int TenantsCreated { get; set; }
    public int DistributorsCreated { get; set; }
    public bool IsDryRun { get; set; }

    /// <summary>
    /// Created user credentials. Distribute securely — passwords are returned only once.
    /// </summary>
    public List<ImportedUserRecord> CreatedUsers { get; set; } = [];
    public List<ImportErrorDto> Errors { get; set; } = [];
}

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

public class ImportedUserRecord
{
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Plain-text password — only returned at creation time.
    /// Format: {username}$$$ (unique username) or {username}$$${digits} (conflicting username).
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
