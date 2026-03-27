using System.ComponentModel;
using System.Data;
using System.Reflection;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Alien.Common.Excel;

public class ExcelWriter : IExcelWriter, IDisposable
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

    public byte[] Export(DataTable source)
    {
        ISheet temp = workbook.CreateSheet(source.TableName ?? "Sheet1");
        setSheet(temp, source);
        MemoryStream stream = new MemoryStream();
        workbook.Write(stream, false);
        byte[] result = stream.ToArray();
        stream.Dispose();
        return result;
    }

    public byte[] Export(DataSet source)
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

    public byte[] Export<T>(List<T> source)
    {
        ISheet temp = workbook.CreateSheet(typeof(T).Name);
        setSheet(temp, source);
        MemoryStream stream = new MemoryStream();
        workbook.Write(stream, false);
        byte[] result = stream.ToArray();
        stream.Dispose();
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
