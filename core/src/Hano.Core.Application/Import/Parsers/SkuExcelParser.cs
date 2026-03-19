using Hano.Core.Import.Excel;

namespace Hano.Core.Import.Parsers;

/// <param name="Code">SKU_GOC — unique SKU code (idempotency key).</param>
/// <param name="ProductName">TEN_SAN_PHAM_CHUAN — standard product name (without volume).</param>
/// <param name="VolumeVal">DUNG_TICH_VAL — numeric volume value e.g. "110".</param>
/// <param name="VolumeUnit">DUNG_TICH_UNIT — volume unit e.g. "ml".</param>
/// <param name="Category">DONG_SAN_PHAM — product line / category.</param>
/// <param name="Brand">THUONG_NHAN — brand / manufacturer.</param>
public record SkuRow(
    string Code,
    string ProductName,
    string VolumeVal,
    string VolumeUnit,
    string Category,
    string Brand);

/// <summary>
/// Parses the "SKU.xlsx" file.
/// Expected: 1 sheet "Sheet1", header row 0, data from row 1.
/// Relevant columns: SKU_GOC, TEN_SAN_PHAM_CHUAN, DUNG_TICH_VAL, DUNG_TICH_UNIT,
///                   DONG_SAN_PHAM, THUONG_NHAN.
/// </summary>
public class SkuExcelParser(IExcelReader reader)
{
    public IEnumerable<SkuRow> Parse(Stream stream)
    {
        var rows = reader.ReadSheet(stream, "Sheet1");
        if (rows.Count < 2) yield break;

        var header = rows[0];
        int codeCol = FindColumn(header, "sku_goc");
        int nameCol = FindColumn(header, "ten_san_pham_chuan");
        int volValCol = FindColumn(header, "dung_tich_val");
        int volUnitCol = FindColumn(header, "dung_tich_unit");
        int categoryCol = FindColumn(header, "dong_san_pham");
        int brandCol = FindColumn(header, "thuong_nhan");

        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            var code = Cell(row, codeCol);
            if (string.IsNullOrWhiteSpace(code)) continue;

            yield return new SkuRow(
                Code: code.Trim(),
                ProductName: Cell(row, nameCol),
                VolumeVal: Cell(row, volValCol),
                VolumeUnit: Cell(row, volUnitCol),
                Category: Cell(row, categoryCol),
                Brand: Cell(row, brandCol));
        }
    }

    private static int FindColumn(IReadOnlyList<object?> headerRow, string name)
    {
        for (int i = 0; i < headerRow.Count; i++)
        {
            var cell = headerRow[i]?.ToString()?.ToLowerInvariant().Trim() ?? string.Empty;
            if (cell == name) return i;
        }
        return -1;
    }

    private static string Cell(IReadOnlyList<object?> row, int index)
    {
        if (index < 0 || index >= row.Count) return string.Empty;
        return row[index]?.ToString()?.Trim() ?? string.Empty;
    }
}
