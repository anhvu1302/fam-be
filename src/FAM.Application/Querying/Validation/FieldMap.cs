using System.Linq.Expressions;

namespace FAM.Application.Querying.Validation;

/// <summary>
/// FieldMap định nghĩa whitelist các field có thể query/sort và type của chúng
/// </summary>
public sealed class FieldMap<T>
{
    private readonly Dictionary<string, FieldMapping> _map = new(StringComparer.OrdinalIgnoreCase);

    public FieldMap<T> Add<TProp>(
        string name, 
        Expression<Func<T, TProp>> selector, 
        bool canFilter = true, 
        bool canSort = true, 
        bool canSelect = true)
    {
        _map[name] = new FieldMapping(selector, typeof(TProp), canFilter, canSort, canSelect);
        return this;
    }

    public bool TryGet(string name, out LambdaExpression expression, out Type type)
    {
        if (_map.TryGetValue(name, out var mapping))
        {
            expression = mapping.Expression;
            type = mapping.Type;
            return true;
        }

        expression = null!;
        type = null!;
        return false;
    }

    public bool CanFilter(string name)
    {
        return _map.TryGetValue(name, out var mapping) && mapping.CanFilter;
    }

    public bool CanSort(string name)
    {
        return _map.TryGetValue(name, out var mapping) && mapping.CanSort;
    }

    public bool CanSelect(string name)
    {
        return _map.TryGetValue(name, out var mapping) && mapping.CanSelect;
    }

    public bool ContainsField(string name)
    {
        return _map.ContainsKey(name);
    }

    public IEnumerable<string> GetFieldNames() => _map.Keys;

    private sealed record FieldMapping(
        LambdaExpression Expression,
        Type Type,
        bool CanFilter,
        bool CanSort,
        bool CanSelect);
}
