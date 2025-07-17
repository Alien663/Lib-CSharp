using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Alien.Common.Utility;

public static class ListExtensions
{
    public static DataTable ToDataTable<T>(List<T> items) where T : class
    {
        var result = new DataTable(typeof(T).Name);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Create Columns
        foreach(var prop in props)
        {
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            result.Columns.Add(prop.Name, propType);
        }

        // Data filling
        foreach(var item in items)
        {
            var row = result.NewRow();
            foreach (var prop in props)
            {
                var value = prop.GetValue(item, null) ?? DBNull.Value;
                row[prop.Name] = value;
            }
            result.Rows.Add(row);
        }
        return result;
    }
}
