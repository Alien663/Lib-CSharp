using System.Data;
using System.Reflection;

namespace Alien.Common.Utility;

public static class DataTableExtensions
{
    /// <summary>
    /// 將 DataTable 轉換為指定類型的物件列表
    /// </summary>
    /// <typeparam name="T">目標物件類型</typeparam>
    /// <param name="table">要轉換的 DataTable</param>
    /// <returns>轉換後的物件列表</returns>
    /// <exception cref="ArgumentNullException">當 table 為 null 時拋出</exception>
    public static IList<T> ToList<T>(this DataTable table) where T : new()
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        var result = new List<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // 建立屬性名稱對應的字典，提升查找效能
        var propertyDict = properties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        
        foreach (DataRow row in table.Rows)
        {
            var obj = new T();
            
            foreach (DataColumn column in table.Columns)
            {
                if (!propertyDict.TryGetValue(column.ColumnName, out var property))
                    continue;
                
                if (row[column] == DBNull.Value)
                    continue;

                try
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var value = Convert.ChangeType(row[column], targetType);
                    property.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    // 可以考慮記錄轉換失敗的資訊
                    System.Diagnostics.Debug.WriteLine($"轉換失敗: 屬性 {property.Name}, 值 {row[column]}, 錯誤: {ex.Message}");
                }
            }
            
            result.Add(obj);
        }
        
        return result;
    }

    /// <summary>
    /// 將 DataTable 轉換為指定類型的物件列表，並提供轉換失敗的回調
    /// </summary>
    /// <typeparam name="T">目標物件類型</typeparam>
    /// <param name="table">要轉換的 DataTable</param>
    /// <param name="onConversionError">轉換失敗時的回調函數</param>
    /// <returns>轉換後的物件列表</returns>
    public static IList<T> ToList<T>(this DataTable table, Action<string, object, Exception>? onConversionError = null) where T : new()
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        var result = new List<T>();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propertyDict = properties.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        
        foreach (DataRow row in table.Rows)
        {
            var obj = new T();
            
            foreach (DataColumn column in table.Columns)
            {
                if (!propertyDict.TryGetValue(column.ColumnName, out var property))
                    continue;
                
                if (row[column] == DBNull.Value)
                    continue;

                try
                {
                    var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var value = Convert.ChangeType(row[column], targetType);
                    property.SetValue(obj, value);
                }
                catch (Exception ex)
                {
                    onConversionError?.Invoke(property.Name, row[column], ex);
                }
            }
            
            result.Add(obj);
        }
        
        return result;
    }
}
