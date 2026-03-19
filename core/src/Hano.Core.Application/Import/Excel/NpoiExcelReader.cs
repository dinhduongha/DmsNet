using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Hano.Core.Import.Excel;

/// <summary>NPOI reader for .xlsx files (XSSFWorkbook).</summary>
public class NpoiExcelReader : IExcelReader
{
    public IReadOnlyList<string> GetSheetNames(Stream stream)
    {
        stream.Position = 0;
        IWorkbook wb = new XSSFWorkbook(stream);
        var names = new List<string>();
        for (int i = 0; i < wb.NumberOfSheets; i++)
            names.Add(wb.GetSheetName(i));
        return names;
    }

    public IReadOnlyList<IReadOnlyList<object?>> ReadSheet(Stream stream, string sheetName)
    {
        stream.Position = 0;
        IWorkbook wb = new XSSFWorkbook(stream);
        ISheet sheet = wb.GetSheet(sheetName)
            ?? throw new InvalidOperationException($"Sheet '{sheetName}' not found.");

        // Determine max column count across all rows
        int maxCol = 0;
        for (int r = 0; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row != null && row.LastCellNum > maxCol)
                maxCol = row.LastCellNum;
        }

        var result = new List<IReadOnlyList<object?>>();
        for (int r = 0; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            var cells = new List<object?>();
            for (int c = 0; c < maxCol; c++)
                cells.Add(GetCellValue(row?.GetCell(c)));
            result.Add(cells);
        }

        return result;
    }

    private static object? GetCellValue(ICell? cell)
    {
        if (cell == null) return null;
        return cell.CellType switch
        {
            CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                ? cell.DateCellValue
                : (object)cell.NumericCellValue,
            CellType.String => cell.StringCellValue,
            CellType.Boolean => cell.BooleanCellValue,
            CellType.Formula => cell.CachedFormulaResultType == CellType.Numeric
                ? (object)cell.NumericCellValue
                : cell.StringCellValue,
            _ => null
        };
    }
}
