using System.Linq.Expressions;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// Base repository with common helper methods
/// Pragmatic Architecture - uses single Domain entity without mapping layer
/// </summary>
public abstract class BasePagedRepository<TEntity>
    where TEntity : class
{
    protected BasePagedRepository()
    {
    }

    /// <summary>
    /// Extract property name from expression for Include mapping
    /// </summary>
    protected string GetPropertyName(Expression expression)
    {
        // Handle Convert expression (when casting to object)
        if (expression is UnaryExpression unaryExpression &&
            unaryExpression.NodeType == ExpressionType.Convert)
            expression = unaryExpression.Operand;

        // Handle member access
        if (expression is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new InvalidOperationException($"Cannot extract property name from expression: {expression}");
    }

    /// <summary>
    /// Parse include string and return corresponding expressions with validation
    /// </summary>
    protected static Expression<Func<TEntity, object>>[] ParseIncludes(
        string? includeString,
        Dictionary<string, Expression<Func<TEntity, object>>> allowedIncludes)
    {
        if (string.IsNullOrWhiteSpace(includeString))
            return Array.Empty<Expression<Func<TEntity, object>>>();

        var includeNames = includeString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        var expressions = new List<Expression<Func<TEntity, object>>>();

        foreach (var includeName in includeNames)
            if (allowedIncludes.TryGetValue(includeName, out Expression<Func<TEntity, object>>? expression))
                expressions.Add(expression);
            else
                throw new InvalidOperationException(
                    $"Include '{includeName}' is not allowed. Allowed includes: {string.Join(", ", allowedIncludes.Keys)}");

        return expressions.ToArray();
    }
}
