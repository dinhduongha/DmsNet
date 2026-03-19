using MiniExcelLibs;

namespace Hano.Core.Import.Excel;

public class MiniExcelReader : IExcelReader
{
    public IReadOnlyList<string> GetSheetNames(Stream stream)
    {
        stream.Position = 0;
        return MiniExcel.GetSheetNames(stream);
    }

    public IReadOnlyList<IReadOnlyList<object?>> ReadSheet(Stream stream, string sheetName)
    {
        stream.Position = 0;
        var result = new List<IReadOnlyList<object?>>();

        // useHeaderRow: false → keys are column letters "A","B","C"...
        var rows = MiniExcel.Query(stream, sheetName: sheetName, useHeaderRow: false);

        foreach (IDictionary<string, object?> row in rows)
        {
            // Sort by column letter to guarantee left-to-right order
            var cells = row
                .OrderBy(kvp => ColToIndex(kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();
            result.Add(cells);
        }

        return result;
    }

    /// <summary>Converts Excel column letter(s) to 0-based index. A=0, B=1, Z=25, AA=26.</summary>
    private static int ColToIndex(string col)
    {
        int result = 0;
        foreach (char c in col.ToUpperInvariant())
            result = result * 26 + (c - 'A' + 1);
        return result - 1;
    }
}
