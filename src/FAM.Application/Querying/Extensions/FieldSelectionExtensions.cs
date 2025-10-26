using System.Dynamic;
using System.Reflection;
using System.Text.Json;

namespace FAM.Application.Querying.Extensions;

/// <summary>
/// Extensions for field selection/projection on DTOs
/// </summary>
public static class FieldSelectionExtensions
{
    /// <summary>
    /// Apply field selection to a list of DTOs
    /// Returns only the specified fields for each DTO
    /// </summary>
    public static List<Dictionary<string, object?>> SelectFields<T>(
        this IEnumerable<T> items,
        string[]? fields) where T : class
    {
        var result = new List<Dictionary<string, object?>>();

        if (fields == null || fields.Length == 0)
        {
            // No field selection - return all properties as dictionaries
            var allProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var item in items)
            {
                var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in allProperties)
                {
                    dict[ToCamelCase(prop.Name)] = prop.GetValue(item);
                }
                result.Add(dict);
            }
            
            return result;
        }

        // Field selection specified
        var selectedProperties = new List<PropertyInfo>();
        var type = typeof(T);

        foreach (var fieldName in fields)
        {
            var prop = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                selectedProperties.Add(prop);
            }
        }

        foreach (var item in items)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in selectedProperties)
            {
                dict[ToCamelCase(prop.Name)] = prop.GetValue(item);
            }
            result.Add(dict);
        }

        return result;
    }

    /// <summary>
    /// Apply field selection to a PageResult
    /// </summary>
    public static PageResult<Dictionary<string, object?>> SelectFields<T>(
        this PageResult<T> pageResult,
        string[]? fields) where T : class
    {
        var selectedItems = pageResult.Items.SelectFields(fields);
        return new PageResult<Dictionary<string, object?>>(
            selectedItems,
            pageResult.Page,
            pageResult.PageSize,
            pageResult.Total);
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}
