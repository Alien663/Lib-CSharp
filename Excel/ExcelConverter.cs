using System.ComponentModel;
using System.Data;
using System.Numerics;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Alien.Common.Excel;

public class ExcelReader : IDisposable
{
    private bool _disposed;
    private IWorkbook workbook;
    private AnchorModel anchor;
    private DataRangeModel dataRange;
    private SheetRangeModel sheetRange;

    public ExcelReader()
    {
        workbook = new XSSFWorkbook();
        anchor = new AnchorModel();
        dataRange = new DataRangeModel();
        sheetRange = new SheetRangeModel();
    }
    public DataTable readFileDT(MemoryStream fs)
    {
        try
        {
            workbook = new XSSFWorkbook(fs);
            DataTable result = readSheet(sheetRange.StartIndex);
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read Excel file", ex);
        }
        finally
        {
            fs.Close();
        }
    }

    public DataSet readFileDS(MemoryStream fs)
    {
        try
        {
            workbook = new XSSFWorkbook(fs);
            DataSet result = new DataSet();
            for (int i = sheetRange.StartIndex; i < (sheetRange.EndIndex == 0 ? workbook.NumberOfSheets : sheetRange.EndIndex); i++)
            {
                DataTable dt = readSheet(i);
                result.Tables.Add(dt);
            }
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read Excel file", ex);
        }
        finally
        {
            fs.Close();
        }
    }

    public List<T> readFileDM<T>(MemoryStream fs) where T : new()
    {
        workbook = new XSSFWorkbook(fs);
        ISheet sheet = workbook.GetSheetAt(sheetRange.StartIndex);
        IRow row = sheet.GetRow(anchor.CellY);
        List<T> dmResult = new List<T>();
        List<string> columns = new List<string>();

        for (int i = anchor.CellX; i < (dataRange.RangeX == 0 ? row.LastCellNum : anchor.CellX + dataRange.RangeX); i++)
        {
            columns.Add(row.GetCell(i).ToString());
        }

        for (int i = anchor.CellY + 1; i <= (dataRange.RangeY == 0 ? sheet.LastRowNum : anchor.CellY + dataRange.RangeY); i++)
        {
            T t = new T();
            PropertyInfo[] propertys = t.GetType().GetProperties();
            row = sheet.GetRow(i);
            if (row != null)
            {
                foreach (PropertyInfo pi in propertys)
                {
                    var attr = pi.GetCustomAttribute<DisplayNameAttribute>(false);
                    if (columns.Contains(pi.Name) || columns.Contains(attr.DisplayName))
                    {
                        if (!pi.CanWrite) continue;
                        ICell cell = columns.IndexOf(pi.Name) == -1 ?
                            row.GetCell(anchor.CellX + columns.IndexOf(attr.DisplayName)) :
                            row.GetCell(anchor.CellX + columns.IndexOf(pi.Name));

                        string value = "";
                        switch (cell.CellType)
                        {
                            case CellType.String:
                                value = cell.StringCellValue;
                                break;
                            case CellType.Numeric:
                                value = cell.NumericCellValue.ToString();
                                break;
                            case CellType.Boolean:
                                value = cell.BooleanCellValue.ToString();
                                break;
                            case CellType.Formula:
                                value = cell.CachedFormulaResultType.ToString();
                                break;
                            case CellType.Unknown:
                                value = cell.StringCellValue;
                                break;
                            case CellType.Error:
                                value = cell.ErrorCellValue.ToString();
                                break;
                        }
                        pi.SetValue(t, ConvertValue(value, pi.PropertyType), null);
                    }
                }
                dmResult.Add(t);
            }
        }
        return dmResult;
    }
    private object ConvertValue(string value, Type targetType)
    {
        return targetType.Name switch
        {
            "UInt16" => Convert.ToUInt16(value),
            "UInt32" => Convert.ToUInt32(value),
            "UInt64" => Convert.ToUInt64(value),
            "Int16" => Convert.ToInt16(value),
            "Int32" => Convert.ToInt32(value),
            "Int64" => Convert.ToInt64(value),
            "Single" => Convert.ToSingle(value),
            "Double" => Convert.ToDouble(value),
            "Decimal" => Convert.ToDecimal(value),
            "BigInteger" => BigInteger.Parse(value),
            "Boolean" => Convert.ToBoolean(value),
            "DateTime" => Convert.ToDateTime(value),
            "DateOnly" => DateOnly.Parse(value),
            "TimeOnly" => TimeOnly.Parse(value),
            _ => value,
        };
    }
    private DataTable readSheet(int sheetIndex)
    {
        ISheet sheet = workbook.GetSheetAt(sheetIndex);
        IRow row = sheet.GetRow(anchor.CellY);
        DataTable dt = new DataTable();
        dt.TableName = sheet.SheetName;

        for (int i = anchor.CellX; i < (dataRange.RangeX == 0 ? row.LastCellNum : anchor.CellX + dataRange.RangeX); i++)
        {
            string cellValue = row.GetCell(i).ToString();
            dt.Columns.Add(cellValue);
        }
        for (int i = 0; i < (dataRange.RangeY == 0 ? sheet.LastRowNum - anchor.CellY : dataRange.RangeY); i++)
        {
            DataRow dr = dt.NewRow();
            row = sheet.GetRow(i + 1 + anchor.CellY);
            if (row != null)
            {
                for (int j = 0; j < (dataRange.RangeX == 0 ? row.LastCellNum - anchor.CellX : dataRange.RangeX); j++)
                {
                    var cell = row.GetCell(j + anchor.CellX);
                    switch (cell.CellType)
                    {
                        case CellType.Numeric when DateUtil.IsCellDateFormatted(cell):
                            dr[j] = cell.DateCellValue;
                            break;
                        case CellType.Formula:
                            HSSFFormulaEvaluator eva = new HSSFFormulaEvaluator(workbook);
                            dr[j] = eva.Evaluate(cell).StringValue;
                            break;
                        case CellType.Numeric:
                            dr[j] = cell.NumericCellValue;
                            break;
                        case CellType.Boolean:
                            dr[j] = cell.BooleanCellValue;
                            break;
                        case CellType.Error:
                            dr[j] = cell.ErrorCellValue;
                            break;
                        default:
                            dr[j] = cell.StringCellValue;
                            break;
                    }
                }
            }
            dt.Rows.Add(dr);
        }
        return dt;
    }
    
    public void setAnchor(int x, int y)
    {
        anchor.CellX = x;
        anchor.CellY = y;
    }

    public void setDataRange(int columns, int rows)
    {
        dataRange.RangeX = columns;
        dataRange.RangeY = rows;
    }

    public void setSheetRange(int start, int end)
    {
        sheetRange.StartIndex = start;
        sheetRange.EndIndex = end;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                workbook.Dispose();
            }
            _disposed = true;
        }
    }
}

public class ExcelWriter : IDisposable
{
    private bool _disposed;
    private IWorkbook workbook;
    private AnchorModel anchor;
    private DataRangeModel dataRange;
    private SheetRangeModel sheetRange;
    private Dictionary<string, string> DataTypeStyle;
    public ExcelWriter(Dictionary<string, string>? customDataTypeStyle = null)
    {
        workbook = new XSSFWorkbook();
        anchor = new AnchorModel();
        dataRange = new DataRangeModel();
        sheetRange = new SheetRangeModel();
        DataTypeStyle = customDataTypeStyle ?? new Dictionary<string, string>
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
    public byte[] export(DataTable source)
    {
        ISheet temp = workbook.CreateSheet(source.TableName ?? "Sheet1");
        setSheet(temp, source);
        MemoryStream stream = new MemoryStream();
        workbook.Write(stream, false);
        byte[] result = stream.ToArray();
        stream.Dispose();
        return result;
    }

    public byte[] export(DataSet source)
    {
        for (int i = sheetRange.StartIndex; i < (sheetRange.EndIndex == 0 ? source.Tables.Count : sheetRange.EndIndex); i++)
        {
            DataTable dt = source.Tables[i];
            ISheet temp = workbook.CreateSheet(source.Tables[i].TableName ?? $"Sheet{i}");
            setSheet(temp, source.Tables[i]);
        }
        MemoryStream stream = new MemoryStream();
        workbook.Write(stream, false);
        byte[] result = stream.ToArray();
        stream.Close();
        return result;
    }

    public byte[] export<T>(List<T> items)
    {
        ISheet temp = workbook.CreateSheet(typeof(T).Name);
        setSheet(temp, items);
        MemoryStream stream = new MemoryStream();
        workbook.Write(stream, false);
        byte[] result = stream.ToArray();
        stream.Dispose();
        return result;
    }
    private void setSheet(ISheet sheet, DataTable source)
    {
        if (anchor.CellY >= 0)
        {
            IRow header = sheet.CreateRow(anchor.CellY);
            for (int i = 0; i < source.Columns.Count; i++)
            {
                ICell cell = header.CreateCell(i + anchor.CellX);
                cell.SetCellValue(source.Columns[i].ColumnName);
            }
        }
        for (int i = 0; i < source.Rows.Count; i++)
        {
            IRow rows = sheet.CreateRow(i + anchor.CellY + 1);
            for (int j = 0; j < source.Columns.Count; j++)
            {
                ICell cell = rows.CreateCell(j + anchor.CellX);
                if (DataTypeStyle.ContainsKey(source.Columns[j].DataType.Name))
                {
                    ICellStyle _datastyle = workbook.CreateCellStyle();
                    _datastyle.DataFormat = workbook.CreateDataFormat()
                                .GetFormat(DataTypeStyle[source.Columns[j].DataType.Name]);
                    cell.CellStyle = _datastyle;
                }
                SetCellValue(cell, source.Rows[i][j], source.Columns[j].DataType.Name);
            }
        }
    }

    private void setSheet<T>(ISheet sheet, List<T> source)
    {
        PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        int i = anchor.CellX;
        if (anchor.CellY >= 0)
        {
            IRow header = sheet.CreateRow(anchor.CellY);
            foreach (PropertyInfo prop in Props)
            {
                ICell cell = header.CreateCell(i++);
                var attr = prop.GetCustomAttribute<DisplayNameAttribute>(false);
                if (attr == null)
                    cell.SetCellValue(prop.Name);
                else
                    cell.SetCellValue(attr.DisplayName);
            }
        }
        i = anchor.CellY;
        foreach (var item in source)
        {
            IRow rows = sheet.CreateRow(++i);
            for (int j = 0; j < Props.Length; j++)
            {
                ICell cell = rows.CreateCell(j + anchor.CellX);
                if (DataTypeStyle.ContainsKey(Props[j].PropertyType.Name))
                {
                    ICellStyle _datastyle = workbook.CreateCellStyle();
                    _datastyle.DataFormat = workbook.CreateDataFormat()
                                .GetFormat(DataTypeStyle[Props[j].PropertyType.Name]);
                    cell.CellStyle = _datastyle;
                }
                var cellValue = Props[j].GetValue(item, null);

                SetCellValue(cell, cellValue, Props[j].PropertyType.Name);
            }
        }
    }
    private void SetCellValue(ICell cell, object value, string datatype)
    {
        switch (datatype)
        {
            case "UInt16":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToUInt16(value));
                break;
            case "UInt32":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToUInt32(value));
                break;
            case "UInt64":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToUInt64(value));
                break;
            case "Int16":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToInt16(value));
                break;
            case "Int32":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToInt32(value));
                break;
            case "Int64":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToInt64(value));
                break;
            case "Boolean":
                cell.SetCellType(CellType.Boolean);
                cell.SetCellValue(Convert.ToBoolean(value));
                break;
            case "Float":
            case "Double":
            case "Decimal":
                cell.SetCellType(CellType.Numeric);
                cell.SetCellValue(Convert.ToDouble(value));
                break;
            default:
                cell.SetCellType(CellType.String);
                cell.SetCellValue(value.ToString());
                break;
        }
    }
    public void setAnchor(int x, int y)
    {
        anchor.CellX = x;
        anchor.CellY = y;
    }

    public void setDataRange(int columns, int rows)
    {
        dataRange.RangeX = columns;
        dataRange.RangeY = rows;
    }

    public void setSheetRange(int start, int end)
    {
        sheetRange.StartIndex = start;
        sheetRange.EndIndex = end;
    }
    public void setDataTypeStyle(Dictionary<string, string> pairs)
    {
        foreach (string pair in pairs.Keys)
        {
            if (DataTypeStyle.ContainsKey(pair))
            {
                DataTypeStyle[pair] = pairs[pair];
            }
            else
            {
                DataTypeStyle.Add(pair, pairs[pair]);
            }
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                workbook.Dispose();
            }
            _disposed = true;
        }
    }
}
