using System.ComponentModel;
using System.Data;
using System.Numerics;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Alien.Common.Excel;

public class ExcelReader : IExcelReader, IDisposable
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

    public DataTable ToDataTable(byte[] file)
    {
        using var ms = new MemoryStream(file);
        try
        {
            workbook = new XSSFWorkbook(ms);
            DataTable result = readSheet(sheetRange.StartIndex);
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read Excel file", ex);
        }
        finally
        {
            ms.Close();
        }

    }
    public DataSet ToDataSet(byte[] file)
    {
        using var ms = new MemoryStream(file);
        try
        {
            workbook = new XSSFWorkbook(ms);
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
            ms.Close();
        }

    }
    public List<T> ToList<T>(byte[] file) where T : new()
    {
        using var ms = new MemoryStream(file);
        workbook = new XSSFWorkbook(ms);
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
