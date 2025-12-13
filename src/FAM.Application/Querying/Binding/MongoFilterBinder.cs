using System.Linq.Expressions;
using System.Text.RegularExpressions;

using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Validation;

using MongoDB.Bson;
using MongoDB.Driver;

namespace FAM.Application.Querying.Binding;

/// <summary>
/// Bind filter AST thành FilterDefinition<T> cho MongoDB
/// </summary>
public static class MongoFilterBinder<T>
{
    public static FilterDefinition<T> Bind(FilterNode node, FieldMap<T> fieldMap)
    {
        FilterDefinitionBuilder<T>? builder = Builders<T>.Filter;
        return BindNode(node, builder, fieldMap);
    }

    private static FilterDefinition<T> BindNode(
        FilterNode node,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        return node switch
        {
            BinaryNode binary => BindBinary(binary, builder, fieldMap),
            UnaryNode unary => BindUnary(unary, builder, fieldMap),
            CallNode call => BindCall(call, builder, fieldMap),
            GroupNode group => BindNode(group.Expression, builder, fieldMap),
            _ => throw new NotSupportedException($"Node type {node.GetType().Name} cannot be used at this position")
        };
    }

    private static FilterDefinition<T> BindBinary(
        BinaryNode node,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        return node.Operator switch
        {
            FilterOperator.And => builder.And(
                BindNode(node.Left, builder, fieldMap),
                BindNode(node.Right, builder, fieldMap)),

            FilterOperator.Or => builder.Or(
                BindNode(node.Left, builder, fieldMap),
                BindNode(node.Right, builder, fieldMap)),

            FilterOperator.Equal => BuildEqual(node.Left, node.Right, builder, fieldMap),
            FilterOperator.NotEqual => BuildNotEqual(node.Left, node.Right, builder, fieldMap),
            FilterOperator.GreaterThan => BuildGreaterThan(node.Left, node.Right, builder, fieldMap),
            FilterOperator.GreaterThanOrEqual => BuildGreaterThanOrEqual(node.Left, node.Right, builder, fieldMap),
            FilterOperator.LessThan => BuildLessThan(node.Left, node.Right, builder, fieldMap),
            FilterOperator.LessThanOrEqual => BuildLessThanOrEqual(node.Left, node.Right, builder, fieldMap),

            _ => throw new NotSupportedException($"Binary operator {node.Operator} not supported for MongoDB")
        };
    }

    private static FilterDefinition<T> BindUnary(
        UnaryNode node,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        return node.Operator switch
        {
            FilterOperator.Not => builder.Not(BindNode(node.Operand, builder, fieldMap)),
            FilterOperator.IsNull => BuildIsNull(node.Operand, builder, fieldMap),
            FilterOperator.NotNull => BuildIsNotNull(node.Operand, builder, fieldMap),
            _ => throw new NotSupportedException($"Unary operator {node.Operator} not supported for MongoDB")
        };
    }

    private static FilterDefinition<T> BindCall(
        CallNode node,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        return node.Operator switch
        {
            FilterOperator.Contains => BuildStringContains(node.Target, node.Arguments[0], builder, fieldMap),
            FilterOperator.NotContains => builder.Not(BuildStringContains(node.Target, node.Arguments[0], builder,
                fieldMap)),
            FilterOperator.StartsWith => BuildStringStartsWith(node.Target, node.Arguments[0], builder, fieldMap),
            FilterOperator.EndsWith => BuildStringEndsWith(node.Target, node.Arguments[0], builder, fieldMap),
            FilterOperator.In => BuildIn(node.Target, node.Arguments, builder, fieldMap),
            FilterOperator.NotIn => BuildNotIn(node.Target, node.Arguments, builder, fieldMap),
            FilterOperator.Between => BuildBetween(node.Target, node.Arguments[0], node.Arguments[1], builder,
                fieldMap),
            FilterOperator.ContainsAny => BuildContainsAny(node.Target, node.Arguments, builder, fieldMap),
            _ => throw new NotSupportedException($"Call operator {node.Operator} not supported for MongoDB")
        };
    }

    // Helper methods

    private static FilterDefinition<T> BuildEqual(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Eq(fieldName, value);
    }

    private static FilterDefinition<T> BuildNotEqual(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Ne(fieldName, value);
    }

    private static FilterDefinition<T> BuildGreaterThan(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Gt(fieldName, value);
    }

    private static FilterDefinition<T> BuildGreaterThanOrEqual(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Gte(fieldName, value);
    }

    private static FilterDefinition<T> BuildLessThan(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Lt(fieldName, value);
    }

    private static FilterDefinition<T> BuildLessThanOrEqual(
        FilterNode left,
        FilterNode right,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (left is not FieldNode field)
            throw new InvalidOperationException("Left operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var value = GetLiteralValue(right);

        return builder.Lte(fieldName, value);
    }

    private static FilterDefinition<T> BuildIsNull(
        FilterNode operand,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (operand is not FieldNode field)
            throw new InvalidOperationException("Operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        return builder.Eq(fieldName, BsonNull.Value);
    }

    private static FilterDefinition<T> BuildIsNotNull(
        FilterNode operand,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (operand is not FieldNode field)
            throw new InvalidOperationException("Operand must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        return builder.Ne(fieldName, BsonNull.Value);
    }

    private static FilterDefinition<T> BuildStringContains(
        FilterNode target,
        FilterNode value,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var searchValue = GetLiteralValue(value)?.ToString() ?? string.Empty;

        // Case-insensitive regex: /value/i
        var regex = new BsonRegularExpression(searchValue, "i");
        return builder.Regex(fieldName, regex);
    }

    private static FilterDefinition<T> BuildStringStartsWith(
        FilterNode target,
        FilterNode value,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var searchValue = GetLiteralValue(value)?.ToString() ?? string.Empty;

        // Case-insensitive regex: /^value/i
        var regex = new BsonRegularExpression($"^{EscapeRegex(searchValue)}", "i");
        return builder.Regex(fieldName, regex);
    }

    private static FilterDefinition<T> BuildStringEndsWith(
        FilterNode target,
        FilterNode value,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var searchValue = GetLiteralValue(value)?.ToString() ?? string.Empty;

        // Case-insensitive regex: /value$/i
        var regex = new BsonRegularExpression($"{EscapeRegex(searchValue)}$", "i");
        return builder.Regex(fieldName, regex);
    }

    private static FilterDefinition<T> BuildIn(
        FilterNode target,
        IReadOnlyList<FilterNode> values,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var valueList = values.Select(GetLiteralValue).ToList();

        return builder.In(fieldName, valueList);
    }

    private static FilterDefinition<T> BuildNotIn(
        FilterNode target,
        IReadOnlyList<FilterNode> values,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var valueList = values.Select(GetLiteralValue).ToList();

        return builder.Nin(fieldName, valueList);
    }

    private static FilterDefinition<T> BuildBetween(
        FilterNode target,
        FilterNode min,
        FilterNode max,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var minValue = GetLiteralValue(min);
        var maxValue = GetLiteralValue(max);

        return builder.And(
            builder.Gte(fieldName, minValue),
            builder.Lte(fieldName, maxValue));
    }

    private static FilterDefinition<T> BuildContainsAny(
        FilterNode target,
        IReadOnlyList<FilterNode> values,
        FilterDefinitionBuilder<T> builder,
        FieldMap<T> fieldMap)
    {
        if (target is not FieldNode field)
            throw new InvalidOperationException("Target must be a field");

        var fieldName = GetFieldName(field, fieldMap);
        var searchValues = values.Select(v => GetLiteralValue(v)?.ToString() ?? string.Empty).ToList();

        // For string fields: combine multiple regex patterns with OR
        var filters = searchValues
            .Select(value => builder.Regex(fieldName, new BsonRegularExpression(EscapeRegex(value), "i")))
            .ToList();

        return builder.Or(filters);
    }

    private static string GetFieldName(FieldNode field, FieldMap<T> fieldMap)
    {
        if (!fieldMap.TryGet(field.Name, out LambdaExpression expression, out _))
            throw new InvalidOperationException($"Field '{field.Name}' not found in field map");

        // Extract property name from expression
        // For MongoDB, we need the actual property name on the document
        // This is a simple implementation - you might need more sophisticated logic
        var memberExpr = expression.Body as MemberExpression;
        return memberExpr?.Member.Name ?? field.Name;
    }

    private static object? GetLiteralValue(FilterNode node)
    {
        if (node is not LiteralNode literal)
            throw new InvalidOperationException("Expected literal value");

        return literal.Value;
    }

    private static string EscapeRegex(string input)
    {
        // Escape special regex characters
        return Regex.Escape(input);
    }
}
