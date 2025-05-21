using System.Data;
using System.Reflection;

namespace Alien.Common.Utility;

public static class DataTableExtensions
{
    public static IList<T> ToList<T>(this DataTable table) where T : new()
    {
        var result = new List<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (DataRow row in table.Rows)
        {
            var obj = new T();
            foreach(var prop in properties)
            {
                if(!table.Columns.Contains(prop.Name) || row[prop.Name] == DBNull.Value) continue;

                try
                {
                    var value = Convert.ChangeType(row[prop.Name], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    prop.SetValue(obj, value);
                }
                catch
                {
                    // Ignore conversion errors
                }
            }
            result.Add(obj);
        }
        return result;
    }
}


// Test Incremental Scanning
// Test Incremental Scanning
// Test Incremental Scanning
// Test Incremental Scanning
