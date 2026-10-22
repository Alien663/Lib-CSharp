using System.ComponentModel;
using System.Data;
using System.Reflection;
using ClosedXML.Excel;

namespace Alien.Common.Excel;

public class ExcelWriter : IExcelWriter, IDisposable
{
    private bool _disposed;
    private readonly AnchorModel anchor;
    private readonly DataRangeModel dataRange;
    private readonly SheetRangeModel sheetRange;
    private readonly Dictionary<string, string> dataTypeStyle;

    public ExcelWriter(Dictionary<string, string>? customDataTypeStyle = null)
    {
        anchor = new AnchorModel();
        dataRange = new DataRangeModel();
        sheetRange = new SheetRangeModel();
        dataTypeStyle = customDataTypeStyle ?? new Dictionary<string, string>
        {
            { "UInt16", "#,##0" },
            { "UInt32", "#,##0" },
            { "UInt64", "#,##0" },
            { "Int16", "#,##0" },
            { "Int32", "#,##0" },
            { "Int64", "#,##0" },
            { "Float", "#,##0.00" },
            { "Double", "#,##0.00" },
            { "Decimal", "#,##0.00" },
        };
    }

    public byte[] Export(DataTable source)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(source.TableName) ? "Sheet1" : source.TableName);
        SetSheet(sheet, source);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] Export(DataSet source)
    {
        using var workbook = new XLWorkbook();

        int start = Math.Max(0, sheetRange.StartIndex);
        int endExclusive = sheetRange.EndIndex == 0
            ? source.Tables.Count
            : Math.Min(sheetRange.EndIndex, source.Tables.Count);

        for (int i = start; i < endExclusive; i++)
        {
            var table = source.Tables[i];
            var sheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(table.TableName) ? $"Sheet{i}" : table.TableName);
            SetSheet(sheet, table);
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] Export<T>(List<T> source)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(typeof(T).Name);
        SetSheet(sheet, source);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public void SetAnchor(int x, int y)
    {
        anchor.CellX = x;
        anchor.CellY = y;
    }

    public void SetDataRange(int columns, int rows)
    {
        dataRange.RangeX = columns;
        dataRange.RangeY = rows;
    }

    public void SetSheetRange(int start, int end)
    {
        sheetRange.StartIndex = start;
        sheetRange.EndIndex = end;
    }

    public void setDataTypeStyle(Dictionary<string, string> pairs)
    {
        foreach (var pair in pairs)
        {
            dataTypeStyle[pair.Key] = pair.Value;
        }
    }

    private void SetSheet(IXLWorksheet sheet, DataTable source)
    {
        int headerRowIndex = anchor.CellY + 1;
        int dataStartRow = headerRowIndex + 1;
        int startCol = anchor.CellX + 1;

        if (anchor.CellY >= 0)
        {
            for (int i = 0; i < source.Columns.Count; i++)
            {
                sheet.Cell(headerRowIndex, startCol + i).Value = source.Columns[i].ColumnName;
            }
        }

        for (int i = 0; i < source.Rows.Count; i++)
        {
            int row = dataStartRow + i;
            for (int j = 0; j < source.Columns.Count; j++)
            {
                var cell = sheet.Cell(row, startCol + j);
                var typeName = source.Columns[j].DataType.Name;

                if (dataTypeStyle.TryGetValue(typeName, out var format))
                {
                    cell.Style.NumberFormat.Format = format;
                }

                SetCellValue(cell, source.Rows[i][j], typeName);
            }
        }
    }

    private void SetSheet<T>(IXLWorksheet sheet, List<T> source)
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        int headerRowIndex = anchor.CellY + 1;
        int dataStartRow = headerRowIndex + 1;
        int startCol = anchor.CellX + 1;

        if (anchor.CellY >= 0)
        {
            for (int i = 0; i < props.Length; i++)
            {
                var attr = props[i].GetCustomAttribute<DisplayNameAttribute>(false);
                sheet.Cell(headerRowIndex, startCol + i).Value = attr?.DisplayName ?? props[i].Name;
            }
        }

        for (int i = 0; i < source.Count; i++)
        {
            int row = dataStartRow + i;
            for (int j = 0; j < props.Length; j++)
            {
                var cell = sheet.Cell(row, startCol + j);
                var typeName = props[j].PropertyType.Name;

                if (dataTypeStyle.TryGetValue(typeName, out var format))
                {
                    cell.Style.NumberFormat.Format = format;
                }

                var value = props[j].GetValue(source[i]);
                SetCellValue(cell, value, typeName);
            }
        }
    }

    private static void SetCellValue(IXLCell cell, object? value, string datatype)
    {
        if (value is null)
        {
            cell.Value = string.Empty;
            return;
        }

        switch (datatype)
        {
            case "UInt16":
            case "UInt32":
            case "UInt64":
            case "Int16":
            case "Int32":
            case "Int64":
            case "Single":
            case "Double":
            case "Decimal":
                cell.Value = Convert.ToDouble(value);
                break;
            case "Boolean":
                cell.Value = Convert.ToBoolean(value);
                break;
            case "DateTime":
                cell.Value = Convert.ToDateTime(value);
                break;
            case "DateOnly":
                cell.Value = ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
                break;
            case "TimeOnly":
                cell.Value = ((TimeOnly)value).ToTimeSpan();
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
