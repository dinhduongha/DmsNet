using Hano.Core.Import.Excel;

namespace Hano.Core.Import.Parsers;

/// <param name="RegionSheet">Sheet name (e.g. "HN", "TB").</param>
/// <param name="Seq">Sequential number from TT column.</param>
/// <param name="ShopName">Store/outlet name.</param>
/// <param name="Address">Physical address.</param>
/// <param name="Phone">Contact phone (nullable).</param>
public record CustomerRow(string RegionSheet, int Seq, string ShopName, string Address, string? Phone);

/// <summary>
/// Parses the "Danh sách khách hàng" Excel file.
///
/// Sheet structure per region:
///   - Rows 0–N : merged title / sub-header rows
///   - One row   : actual column headers containing "Tên Shops", "Địa chỉ", "Điện thoại"
///   - Remaining : data rows
///
/// Sheets "TQ" (tổng quát) and "Mẫu DSKH" (template) are skipped.
/// </summary>
public class CustomerExcelParser(IExcelReader reader)
{
    /// <summary>Sheet name → OrganizationUnit display name mapping.</summary>
    public static readonly IReadOnlyDictionary<string, string> SheetToRegion =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "HN",   "Hà Nội" },
            { "TB",   "Tây Bắc" },
            { "ĐB1",  "Đông Bắc 1" },
            { "ĐB2",  "Đông Bắc 2" },
            { "DH",   "Duyên Hải" },
            { "BMT",  "Bắc Miền Trung" },
            { "TN",   "Tây Nguyên" },
            { "MD",   "Miền Đông" },
            { "SBMB", "Sữa Bộ Miền Bắc" },
            { "SBMT", "Sữa Bộ Miền Trung" },
            { "ST",   "Siêu Thị" },
            { "TH",   "Trường Học" },
            { "CN",   "Công Nghiệp" },
        };

    private static readonly HashSet<string> SkipSheets =
        new(["TQ", "Mẫu DSKH"], StringComparer.OrdinalIgnoreCase);

    public IEnumerable<CustomerRow> Parse(Stream stream)
    {
        var sheetNames = reader.GetSheetNames(stream);

        foreach (var sheetName in sheetNames)
        {
            if (SkipSheets.Contains(sheetName)) continue;
            if (!SheetToRegion.ContainsKey(sheetName)) continue;

            var rows = reader.ReadSheet(stream, sheetName);
            if (rows.Count == 0) continue;

            var headerIdx = FindHeaderRowIndex(rows);
            if (headerIdx < 0) continue;

            var header = rows[headerIdx];
            int ttCol = FindColumn(header, "tt");
            int nameCol = FindColumn(header, "tên shops", "ten shops", "shop");
            int addrCol = FindColumn(header, "địa chỉ", "dia chi", "diachi");
            int phoneCol = FindColumn(header, "điện thoại", "dien thoai", "phone", "sdt");

            if (nameCol < 0) continue; // cannot parse without name column

            for (int r = headerIdx + 1; r < rows.Count; r++)
            {
                var row = rows[r];
                var name = Cell(row, nameCol);
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (IsNumeric(name)) continue; // skip numeric placeholders

                int seq = ttCol >= 0 && int.TryParse(Cell(row, ttCol), out int n) ? n : r;

                yield return new CustomerRow(
                    sheetName,
                    seq,
                    name,
                    addrCol >= 0 ? Cell(row, addrCol) : string.Empty,
                    phoneCol >= 0 ? NullIfEmpty(Cell(row, phoneCol)) : null);
            }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Scans the first 10 rows looking for the header row which should contain
    /// a cell with "Tên Shops" or "shop" in its text.
    /// </summary>
    private static int FindHeaderRowIndex(IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        for (int i = 0; i < Math.Min(rows.Count, 10); i++)
        {
            var rowText = string.Join(" ", rows[i].Select(c => c?.ToString() ?? "")).ToLowerInvariant();
            if (rowText.Contains("tên shops") || rowText.Contains("ten shop"))
                return i;
        }
        return -1;
    }

    /// <summary>Finds the first column whose header contains any of the candidate strings (case-insensitive).</summary>
    private static int FindColumn(IReadOnlyList<object?> headerRow, params string[] candidates)
    {
        for (int i = 0; i < headerRow.Count; i++)
        {
            var cell = headerRow[i]?.ToString()?.ToLowerInvariant().Trim() ?? string.Empty;
            if (candidates.Any(c => cell.Contains(c)))
                return i;
        }
        return -1;
    }

    private static string Cell(IReadOnlyList<object?> row, int index)
    {
        if (index < 0 || index >= row.Count) return string.Empty;
        return row[index]?.ToString()?.Trim() ?? string.Empty;
    }

    private static string? NullIfEmpty(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static bool IsNumeric(string value) =>
        double.TryParse(value, out _);
}
