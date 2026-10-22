using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using ClosedXML.Excel;

namespace Alien.Common.Excel;

public class ExcelReader : IExcelReader, IDisposable
{
    private bool _disposed;
    private readonly AnchorModel anchor;
    private readonly DataRangeModel dataRange;
    private readonly SheetRangeModel sheetRange;

    public ExcelReader()
    {
        anchor = new AnchorModel();
        dataRange = new DataRangeModel();
        sheetRange = new SheetRangeModel();
    }

    public DataTable ToDataTable(byte[] file)
    {
        using var ms = new MemoryStream(file);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet(sheetRange.StartIndex + 1);
        return ReadSheet(worksheet);
    }

    public DataSet ToDataSet(byte[] file)
    {
        using var ms = new MemoryStream(file);
        using var workbook = new XLWorkbook(ms);

        var result = new DataSet();
        int start = Math.Max(0, sheetRange.StartIndex);
        int endExclusive = sheetRange.EndIndex == 0
            ? workbook.Worksheets.Count
            : Math.Min(sheetRange.EndIndex, workbook.Worksheets.Count);

        for (int i = start; i < endExclusive; i++)
        {
            var worksheet = workbook.Worksheet(i + 1);
            result.Tables.Add(ReadSheet(worksheet));
        }

        return result;
    }

    public List<T> ToList<T>(byte[] file) where T : new()
    {
        using var ms = new MemoryStream(file);
        using var workbook = new XLWorkbook(ms);

        var sheet = workbook.Worksheet(sheetRange.StartIndex + 1);
        var headerRowIndex = anchor.CellY + 1;

        var headerRow = sheet.Row(headerRowIndex);
        var lastColumn = dataRange.RangeX == 0
            ? (headerRow.LastCellUsed()?.Address.ColumnNumber ?? anchor.CellX + 1)
            : anchor.CellX + dataRange.RangeX;

        var columns = new List<string>();
        for (int col = anchor.CellX + 1; col <= lastColumn; col++)
        {
            columns.Add(sheet.Cell(headerRowIndex, col).GetString());
        }

        int startRow = anchor.CellY + 2;
        int endRow = dataRange.RangeY == 0
            ? (sheet.LastRowUsed()?.RowNumber() ?? anchor.CellY + 1)
            : anchor.CellY + 1 + dataRange.RangeY;

        var result = new List<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        for (int rowIndex = startRow; rowIndex <= endRow; rowIndex++)
        {
            var row = sheet.Row(rowIndex);
            if (row.IsEmpty()) continue;

            var item = new T();

            foreach (var pi in properties)
            {
                if (!pi.CanWrite) continue;

                var displayName = pi.GetCustomAttribute<DisplayNameAttribute>(false)?.DisplayName;
                int index = columns.IndexOf(pi.Name);
                if (index < 0 && !string.IsNullOrWhiteSpace(displayName))
                {
                    index = columns.IndexOf(displayName);
                }
                if (index < 0) continue;

                var cell = sheet.Cell(rowIndex, anchor.CellX + 1 + index);
                var valueText = GetCellString(cell);

                if (string.IsNullOrWhiteSpace(valueText) && IsNullable(pi.PropertyType))
                {
                    pi.SetValue(item, null);
                    continue;
                }

                pi.SetValue(item, ConvertValue(valueText, pi.PropertyType));
            }

            result.Add(item);
        }

        return result;
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

    private DataTable ReadSheet(IXLWorksheet sheet)
    {
        var dt = new DataTable { TableName = sheet.Name };
        int headerRowIndex = anchor.CellY + 1;

        var headerRow = sheet.Row(headerRowIndex);
        int lastColumn = dataRange.RangeX == 0
            ? (headerRow.LastCellUsed()?.Address.ColumnNumber ?? anchor.CellX + 1)
            : anchor.CellX + dataRange.RangeX;

        for (int col = anchor.CellX + 1; col <= lastColumn; col++)
        {
            string columnName = sheet.Cell(headerRowIndex, col).GetString();
            if (string.IsNullOrWhiteSpace(columnName))
            {
                columnName = $"Column{col - anchor.CellX}";
            }
            dt.Columns.Add(columnName);
        }

        int startRow = anchor.CellY + 2;
        int endRow = dataRange.RangeY == 0
            ? (sheet.LastRowUsed()?.RowNumber() ?? anchor.CellY + 1)
            : anchor.CellY + 1 + dataRange.RangeY;

        for (int rowIndex = startRow; rowIndex <= endRow; rowIndex++)
        {
            var dr = dt.NewRow();
            for (int col = anchor.CellX + 1; col <= lastColumn; col++)
            {
                var cell = sheet.Cell(rowIndex, col);
                dr[col - anchor.CellX - 1] = GetCellObjectValue(cell);
            }
            dt.Rows.Add(dr);
        }

        return dt;
    }

    private static object GetCellObjectValue(IXLCell cell)
    {
        if (cell.IsEmpty()) return DBNull.Value;

        return cell.DataType switch
        {
            XLDataType.Boolean => cell.GetBoolean(),
            XLDataType.Number => cell.GetDouble(),
            XLDataType.DateTime => cell.GetDateTime(),
            XLDataType.TimeSpan => cell.GetTimeSpan(),
            _ => cell.GetString()
        };
    }

    private static string GetCellString(IXLCell cell)
    {
        if (cell.IsEmpty()) return string.Empty;

        return cell.DataType switch
        {
            XLDataType.Boolean => cell.GetBoolean().ToString(),
            XLDataType.Number => cell.GetDouble().ToString(CultureInfo.InvariantCulture),
            XLDataType.DateTime => cell.GetDateTime().ToString("O", CultureInfo.InvariantCulture),
            XLDataType.TimeSpan => cell.GetTimeSpan().ToString("c", CultureInfo.InvariantCulture),
            _ => cell.GetString()
        };
    }

    private static bool IsNullable(Type type)
        => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    private static object? ConvertValue(string value, Type targetType)
    {
        var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (actualType == typeof(string)) return value;
        if (string.IsNullOrWhiteSpace(value))
            return Nullable.GetUnderlyingType(targetType) is not null ? null : string.Empty;

        return actualType.Name switch
        {
            "UInt16" => Convert.ToUInt16(value, CultureInfo.InvariantCulture),
            "UInt32" => Convert.ToUInt32(value, CultureInfo.InvariantCulture),
            "UInt64" => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            "Int16" => Convert.ToInt16(value, CultureInfo.InvariantCulture),
            "Int32" => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            "Int64" => Convert.ToInt64(value, CultureInfo.InvariantCulture),
            "Single" => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            "Double" => Convert.ToDouble(value, CultureInfo.InvariantCulture),
            "Decimal" => Convert.ToDecimal(value, CultureInfo.InvariantCulture),
            "BigInteger" => BigInteger.Parse(value, CultureInfo.InvariantCulture),
            "Boolean" => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            "DateTime" => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            "DateOnly" => DateOnly.Parse(value, CultureInfo.InvariantCulture),
            "TimeOnly" => TimeOnly.Parse(value, CultureInfo.InvariantCulture),
            _ => Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture)
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
