using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Alien.Common.Utility;

public static class ListExtensions
{
    /// <summary>
    /// 將 List<T> 轉換為 DataTable
    /// </summary>
    /// <typeparam name="T">物件類型</typeparam>
    /// <param name="items">要轉換的物件列表</param>
    /// <returns>轉換後的 DataTable</returns>
    /// <exception cref="ArgumentNullException">當 items 為 null 時拋出</exception>
    public static DataTable ToDataTable<T>(this IEnumerable<T> items) where T : class
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var result = new DataTable(typeof(T).Name);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // 如果沒有屬性，返回空的 DataTable
        if (props.Length == 0)
            return result;

        // 建立欄位
        foreach (var prop in props)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            result.Columns.Add(prop.Name, propType);
        }

        // 填入資料
        foreach (var item in items)
        {
            if (item == null)
                continue;

            var row = result.NewRow();
            foreach (var prop in props)
            {
                try
                {
                    var value = prop.GetValue(item, null) ?? DBNull.Value;
                    row[prop.Name] = value;
                }
                catch (Exception ex)
                {
                    // 記錄錯誤但不中斷處理
                    System.Diagnostics.Debug.WriteLine($"取得屬性值失敗: {prop.Name}, 錯誤: {ex.Message}");
                    row[prop.Name] = DBNull.Value;
                }
            }
            result.Rows.Add(row);
        }

        return result;
    }

    /// <summary>
    /// 將 List<T> 轉換為 DataTable，並可指定要包含的屬性
    /// </summary>
    /// <typeparam name="T">物件類型</typeparam>
    /// <param name="items">要轉換的物件列表</param>
    /// <param name="includeProperties">要包含的屬性名稱</param>
    /// <returns>轉換後的 DataTable</returns>
    /// <exception cref="ArgumentNullException">當 items 為 null 時拋出</exception>
    public static DataTable ToDataTable<T>(this IEnumerable<T> items, params string[] includeProperties) where T : class
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var result = new DataTable(typeof(T).Name);
        var allProps = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // 篩選要包含的屬性
        var props = includeProperties?.Length > 0 
            ? allProps.Where(p => includeProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase)).ToArray()
            : allProps;

        if (props.Length == 0)
            return result;

        // 建立欄位
        foreach (var prop in props)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            result.Columns.Add(prop.Name, propType);
        }

        // 填入資料
        foreach (var item in items)
        {
            if (item == null)
                continue;

            var row = result.NewRow();
            foreach (var prop in props)
            {
                try
                {
                    var value = prop.GetValue(item, null) ?? DBNull.Value;
                    row[prop.Name] = value;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"取得屬性值失敗: {prop.Name}, 錯誤: {ex.Message}");
                    row[prop.Name] = DBNull.Value;
                }
            }
            result.Rows.Add(row);
        }

        return result;
    }

    /// <summary>
    /// 將 List<T> 轉換為 DataTable，並可自訂欄位名稱
    /// </summary>
    /// <typeparam name="T">物件類型</typeparam>
    /// <param name="items">要轉換的物件列表</param>
    /// <param name="tableName">DataTable 名稱</param>
    /// <returns>轉換後的 DataTable</returns>
    public static DataTable ToDataTable<T>(this IEnumerable<T> items, string tableName) where T : class
    {
        var result = items.ToDataTable();
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            result.TableName = tableName;
        }
        return result;
    }
}
