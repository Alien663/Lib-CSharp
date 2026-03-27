using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alien.Common.Excel;

public interface IExcelWriter
{
    byte[] Export(DataTable source);
    byte[] Export(DataSet source);
    byte[] Export<T>(List<T> source);
    void SetAnchor(int x, int y);
    void SetDataRange(int columns, int rows);
    void SetSheetRange(int start, int end);

    void setDataTypeStyle(Dictionary<string, string> pairs);
}
