using Hano.Core.Import.Excel;

namespace Hano.Core.Import.Parsers;

// ─── Row models ───────────────────────────────────────────────────────────────

/// <param name="Region">Region/OU display name.</param>
/// <param name="AsmName">Full name of the ASM (Trưởng vùng).</param>
public record RegionRow(string Region, string AsmName);

/// <param name="Region">Region display name.</param>
/// <param name="CustomerCode">Mã KH — used as Tenant name and OdsDistributorId.</param>
/// <param name="Name">Distributor legal name.</param>
/// <param name="Province">Province/City.</param>
/// <param name="Address">Full address.</param>
public record DistributorRow(string Region, string CustomerCode, string Name, string Province, string Address);

/// <param name="Region">Region display name.</param>
/// <param name="Name">Full display name of the person.</param>
public record PersonRow(string Region, string Name);

// ─── Parser ──────────────────────────────────────────────────────────────────

/// <summary>
/// Parses the "Danh sách vùng, NPP, GSBH, NVBH, ADMIN" Excel file.
///
/// Expected sheets:
///   "VÙNG &amp; ASM"  — col 0: Vùng, col 1: ASM (Trưởng vùng)
///   "DS NPP"       — col 0: TT, col 1: ĐỊA BÀN, col 2: Mã KH, col 3: TÊN NPP, col 4: TỈNH, col 5: ĐỊA CHỈ
///   "DS GSBH"      — col 0: TT, col 1: ĐỊA BÀN, col 2: Giám sát bán hàng
///   "DS NVBH"      — col 0: TT, col 1: ĐỊA BÀN, col 2: NHÂN VIÊN BÁN HÀNG
///   "DS ADMIN"     — col 0: Vùng,                col 1: ADMIN PHỤ TRÁCH
/// </summary>
public class MasterDataExcelParser(IExcelReader reader)
{
    public ParsedMasterData Parse(Stream stream) => new(
        ParseRegions(stream),
        ParseDistributors(stream),
        ParsePersonSheet(stream, "DS GSBH", nameColIndex: 2),
        ParsePersonSheet(stream, "DS NVBH", nameColIndex: 2),
        ParseAdmins(stream)
    );

    // ── Sheet: VÙNG & ASM ────────────────────────────────────────────────────

    private List<RegionRow> ParseRegions(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "VÙNG & ASM");
        var result = new List<RegionRow>();

        foreach (var row in rows.Skip(1)) // row 0 = header
        {
            var region = Cell(row, 0);
            var asm = Cell(row, 1);
            if (!string.IsNullOrWhiteSpace(region))
                result.Add(new RegionRow(region, asm));
        }

        return result;
    }

    // ── Sheet: DS NPP ────────────────────────────────────────────────────────

    private List<DistributorRow> ParseDistributors(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "DS NPP");
        var result = new List<DistributorRow>();
        string? currentRegion = null;

        foreach (var row in rows.Skip(1))
        {
            var regionCell = Cell(row, 1);
            var code = Cell(row, 2);
            var name = Cell(row, 3);

            if (!string.IsNullOrWhiteSpace(regionCell))
                currentRegion = regionCell;

            if (string.IsNullOrWhiteSpace(name) || currentRegion == null) continue;
            if (IsPlaceholder(name)) continue;

            result.Add(new DistributorRow(
                currentRegion,
                code,
                name,
                Cell(row, 4),
                Cell(row, 5)));
        }

        return result;
    }

    // ── Sheets: DS GSBH / DS NVBH ────────────────────────────────────────────

    /// <summary>
    /// Generic parser for region-person sheets.
    /// Supports carry-forward region: if ĐỊA BÀN cell is empty, use previous non-empty value.
    /// Skips placeholder rows where the name is numeric or zero-value.
    /// </summary>
    private List<PersonRow> ParsePersonSheet(Stream stream, string sheetName, int nameColIndex)
    {
        var rows = reader.ReadSheet(stream, sheetName);
        var result = new List<PersonRow>();
        string? currentRegion = null;

        foreach (var row in rows.Skip(1))
        {
            var regionCell = Cell(row, 1);
            var name = Cell(row, nameColIndex);

            if (!string.IsNullOrWhiteSpace(regionCell))
                currentRegion = regionCell;

            if (string.IsNullOrWhiteSpace(name) || currentRegion == null) continue;
            if (IsPlaceholder(name)) continue;

            result.Add(new PersonRow(currentRegion, name));
        }

        return result;
    }

    // ── Sheet: DS ADMIN ──────────────────────────────────────────────────────

    private List<PersonRow> ParseAdmins(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "DS ADMIN");
        var result = new List<PersonRow>();

        foreach (var row in rows.Skip(1))
        {
            var region = Cell(row, 0);
            var name = Cell(row, 1);
            if (!string.IsNullOrWhiteSpace(region) && !string.IsNullOrWhiteSpace(name))
                result.Add(new PersonRow(region, name));
        }

        return result;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string Cell(IReadOnlyList<object?> row, int index)
    {
        if (index >= row.Count) return string.Empty;
        return row[index]?.ToString()?.Trim() ?? string.Empty;
    }

    /// <summary>Returns true for placeholder values like "0", "00", empty numbers.</summary>
    private static bool IsPlaceholder(string value) =>
        double.TryParse(value, out _);
}

/// <summary>Parsed result from the master data Excel file.</summary>
public record ParsedMasterData(
    List<RegionRow> Regions,
    List<DistributorRow> Distributors,
    List<PersonRow> Supervisors,
    List<PersonRow> Salespeople,
    List<PersonRow> Admins);
