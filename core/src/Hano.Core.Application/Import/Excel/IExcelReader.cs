namespace Hano.Core.Import.Excel;

/// <summary>
/// Abstraction over Excel reading libraries (MiniExcel, EPPlus, NPOI).
/// Each row is returned as an ordered list of cell values (index = column position, 0-based).
/// </summary>
public interface IExcelReader
{
    /// <summary>
    /// Returns all rows from the given sheet.
    /// Row 0 = first physical row (may be title/merged-cell header).
    /// Each inner list contains cell values ordered left-to-right.
    /// </summary>
    IReadOnlyList<IReadOnlyList<object?>> ReadSheet(Stream stream, string sheetName);

    /// <summary>Returns the names of all sheets in the workbook.</summary>
    IReadOnlyList<string> GetSheetNames(Stream stream);
}
