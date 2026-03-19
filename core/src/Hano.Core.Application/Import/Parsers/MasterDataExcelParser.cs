using Hano.Core.Import.Excel;

namespace Hano.Core.Import.Parsers;

// ─── Row models ───────────────────────────────────────────────────────────────

/// <param name="Index">Sequence number from STT column, or 1-based row order if blank.</param>
/// <param name="Region">Region/OU display name.</param>
/// <param name="AsmName">Full name of the ASM (Trưởng vùng).</param>
public record RegionRow(int Index, string Region, string AsmName);

/// <param name="Index">Sequence number from STT column (col 0), or 1-based row order if blank.</param>
/// <param name="Region">Region display name.</param>
/// <param name="CustomerCode">Mã KH — used as Tenant name and OdsDistributorId.</param>
/// <param name="Name">Distributor legal name.</param>
/// <param name="Province">Province/City.</param>
/// <param name="Address">Full address.</param>
public record DistributorRow(int Index, string Region, string CustomerCode, string Name, string Province, string Address);

/// <param name="Index">Sequence number from STT column (col 0), or 1-based row order if blank.</param>
/// <param name="Region">Region display name.</param>
/// <param name="Name">Full display name of the person.</param>
public record PersonRow(int Index, string Region, string Name);

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
    // No STT column — index is 1-based row order.

    private List<RegionRow> ParseRegions(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "VÙNG & ASM");
        var result = new List<RegionRow>();
        var counter = 0;

        foreach (var row in rows.Skip(1)) // row 0 = header
        {
            var region = Cell(row, 0);
            var asm = Cell(row, 1);
            if (!string.IsNullOrWhiteSpace(region))
                result.Add(new RegionRow(++counter, region, asm));
        }
        if (result.Count > 0) result.RemoveAt(0);
        return result;
    }

    // ── Sheet: DS NPP ────────────────────────────────────────────────────────

    private List<DistributorRow> ParseDistributors(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "DS NPP");
        var result = new List<DistributorRow>();
        string? currentRegion = null;
        var counter = 0;
        var dataStart = FindDataStartIndex(rows);

        foreach (var row in rows.Skip(dataStart))
        {
            var stt = Cell(row, 0);
            var regionCell = Cell(row, 1);

            // Carry-forward region from any row with a non-empty ĐỊA BÀN cell
            if (!string.IsNullOrWhiteSpace(regionCell))
                currentRegion = regionCell;

            // Only collect records on data rows (STT is a positive integer)
            if (!IsSequenceNumber(stt) || currentRegion == null) continue;

            var code = Cell(row, 2);
            var name = Cell(row, 3);
            if (string.IsNullOrWhiteSpace(name)) continue;

            var index = int.TryParse(stt, out var n) ? n : ++counter;

            result.Add(new DistributorRow(
                index,
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
    /// Skips all rows before the first row whose STT cell (col 0) is a number.
    /// Supports carry-forward region: if ĐỊA BÀN cell is empty, use previous non-empty value.
    /// Only records rows whose STT is a positive integer.
    /// </summary>
    private List<PersonRow> ParsePersonSheet(Stream stream, string sheetName, int nameColIndex)
    {
        var rows = reader.ReadSheet(stream, sheetName);
        var result = new List<PersonRow>();
        string? currentRegion = null;
        var counter = 0;
        var dataStart = FindDataStartIndex(rows);

        foreach (var row in rows.Skip(dataStart))
        {
            var stt = Cell(row, 0);
            var regionCell = Cell(row, 1);

            // Carry-forward region from any row with a non-empty ĐỊA BÀN cell
            if (!string.IsNullOrWhiteSpace(regionCell))
                currentRegion = regionCell;

            // Only collect records on data rows (STT is a positive integer)
            if (!IsSequenceNumber(stt) || currentRegion == null) continue;

            var name = Cell(row, nameColIndex);
            if (string.IsNullOrWhiteSpace(name)) continue;

            var index = int.TryParse(stt, out var n) ? n : ++counter;
            result.Add(new PersonRow(index, currentRegion, name));
        }

        return result;
    }

    // ── Sheet: DS ADMIN ──────────────────────────────────────────────────────
    // No STT column — index is 1-based row order.

    private List<PersonRow> ParseAdmins(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "DS ADMIN");
        var result = new List<PersonRow>();
        var counter = 0;

        foreach (var row in rows.Skip(1))
        {
            var region = Cell(row, 0);
            var name = Cell(row, 1);
            if (!string.IsNullOrWhiteSpace(region) && !string.IsNullOrWhiteSpace(name))
                result.Add(new PersonRow(++counter, region, name));
        }
        if (result.Count > 0) result.RemoveAt(0);
        return result;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string Cell(IReadOnlyList<object?> row, int index)
    {
        if (index >= row.Count) return string.Empty;
        return row[index]?.ToString()?.Trim() ?? string.Empty;
    }

    /// <summary>Returns true when value is a positive integer — a valid STT.</summary>
    private static bool IsSequenceNumber(string value) =>
        int.TryParse(value, out var n) && n > 0;

    /// <summary>
    /// Returns the index of the first row whose column 0 is a positive integer (data start).
    /// Falls back to 1 if not found.
    /// </summary>
    private static int FindDataStartIndex(IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        for (var i = 0; i < rows.Count; i++)
        {
            if (IsSequenceNumber(Cell(rows[i], 0)))
                return i;
        }
        return 1;
    }
}

/// <summary>Parsed result from the master data Excel file.</summary>
public record ParsedMasterData(
    List<RegionRow> Regions,
    List<DistributorRow> Distributors,
    List<PersonRow> Supervisors,
    List<PersonRow> Salespeople,
    List<PersonRow> Admins);
