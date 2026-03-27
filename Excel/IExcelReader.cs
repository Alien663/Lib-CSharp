using System.Data;

namespace Alien.Common.Excel;

public interface IExcelReader
{
    DataTable ToDataTable(byte[] file);
    DataSet ToDataSet(byte[] file);
    List<T> ToList<T>(byte[] file) where T : new();
    void SetAnchor(int x, int y);
    void SetDataRange(int columns, int rows);
    void SetSheetRange(int start, int end);
}
