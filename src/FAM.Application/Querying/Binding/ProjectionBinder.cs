using System.Linq.Expressions;
using System.Reflection;
using FAM.Application.Querying.Validation;

namespace FAM.Application.Querying.Binding;

/// <summary>
/// Helper to apply field projection/selection to queries
/// Optimizes performance by selecting only requested fields from database
/// </summary>
public static class ProjectionBinder
{
    /// <summary>
    /// Apply field selection (projection) to query
    /// If fields is null/empty, returns all fields
    /// </summary>
    /// <typeparam name="TSource">Source entity type</typeparam>
    /// <typeparam name="TDto">DTO type to project to</typeparam>
    public static IQueryable<TDto> ApplyProjection<TSource, TDto>(
        IQueryable<TSource> query,
        string[]? fields,
        FieldMap<TSource> fieldMap)
        where TDto : class, new()
    {
        // If no fields specified, return all (will need manual mapping later)
        if (fields == null || fields.Length == 0)
            throw new InvalidOperationException(
                "Cannot auto-project without field specification. " +
                "Either specify fields or use manual mapping.");

        // Validate all requested fields exist and are selectable
        foreach (var field in fields)
        {
            if (!fieldMap.TryGet(field, out _, out _))
                throw new InvalidOperationException($"Field '{field}' not found");

            if (!fieldMap.CanSelect(field))
                throw new InvalidOperationException($"Field '{field}' cannot be selected");
        }

        // Build projection expression dynamically
        var parameter = Expression.Parameter(typeof(TSource), "x");
        var dtoType = typeof(TDto);

        // Create member bindings for each requested field
        var bindings = new List<MemberBinding>();

        foreach (var fieldName in fields)
        {
            // Get source expression from field map
            if (!fieldMap.TryGet(fieldName, out var sourceExpression, out _))
                continue;

            // Find matching property in DTO
            var dtoProperty = dtoType.GetProperty(
                fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (dtoProperty == null || !dtoProperty.CanWrite)
                continue;

            // Replace parameter in source expression
            var visitor = new ParameterReplacerVisitor(sourceExpression.Parameters[0], parameter);
            var sourceBody = visitor.Visit(sourceExpression.Body);

            // Convert if types don't match
            if (sourceBody.Type != dtoProperty.PropertyType)
                // Try to convert (e.g., long to int, DateTime to string, etc.)
                if (dtoProperty.PropertyType.IsAssignableFrom(sourceBody.Type))
                    sourceBody = Expression.Convert(sourceBody, dtoProperty.PropertyType);

            bindings.Add(Expression.Bind(dtoProperty, sourceBody));
        }

        // Create: x => new TDto { Field1 = x.Field1, Field2 = x.Field2, ... }
        var memberInit = Expression.MemberInit(Expression.New(dtoType), bindings);
        var lambda = Expression.Lambda<Func<TSource, TDto>>(memberInit, parameter);

        return query.Select(lambda);
    }

    private class ParameterReplacerVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacerVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}