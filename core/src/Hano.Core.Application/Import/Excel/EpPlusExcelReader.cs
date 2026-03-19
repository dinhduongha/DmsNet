using OfficeOpenXml;

namespace Hano.Core.Import.Excel;

/// <summary>EPPlus 4.x reader (GPL-licensed version, no license key needed).</summary>
public class EpPlusExcelReader : IExcelReader
{
    public IReadOnlyList<string> GetSheetNames(Stream stream)
    {
        stream.Position = 0;
        using var package = new ExcelPackage(stream);
        return package.Workbook.Worksheets.Select(ws => ws.Name).ToList();
    }

    public IReadOnlyList<IReadOnlyList<object?>> ReadSheet(Stream stream, string sheetName)
    {
        stream.Position = 0;
        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets[sheetName]
            ?? throw new InvalidOperationException($"Sheet '{sheetName}' not found.");

        var result = new List<IReadOnlyList<object?>>();
        if (ws.Dimension == null) return result;

        for (int row = 1; row <= ws.Dimension.Rows; row++)
        {
            var cells = new List<object?>();
            for (int col = 1; col <= ws.Dimension.Columns; col++)
                cells.Add(ws.Cells[row, col].Value);
            result.Add(cells);
        }

        return result;
    }
}
