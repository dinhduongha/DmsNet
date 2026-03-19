using Hano.Core.Import.Dtos;
using Microsoft.Extensions.Configuration;

namespace Hano.Core.Import.Excel;

/// <summary>
/// Creates the configured IExcelReader implementation.
/// Default is read from appsettings: "Import:ExcelReader" (MiniExcel | EPPlus | NPOI).
/// </summary>
public class ExcelReaderFactory
{
    private readonly ExcelReaderType _defaultType;

    public ExcelReaderFactory(IConfiguration configuration)
    {
        _defaultType = Enum.TryParse<ExcelReaderType>(
            configuration["Import:ExcelReader"], ignoreCase: true, out var parsed)
            ? parsed
            : ExcelReaderType.MiniExcel;
    }

    public IExcelReader Create(ExcelReaderType? type = null) =>
        (type ?? _defaultType) switch
        {
            ExcelReaderType.EPPlus => new EpPlusExcelReader(),
            ExcelReaderType.NPOI => new NpoiExcelReader(),
            _ => new MiniExcelReader()
        };
}
