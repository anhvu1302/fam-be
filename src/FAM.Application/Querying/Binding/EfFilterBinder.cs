using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

using FAM.Application.Querying.Ast;
using FAM.Application.Querying.Validation;

namespace FAM.Application.Querying.Binding;

/// <summary>
/// Bind filter AST thành Expression<Func<T, bool>> cho EF Core
/// </summary>
public static class EfFilterBinder<T>
{
    public static Expression<Func<T, bool>> Bind(FilterNode node, FieldMap<T> fieldMap)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        Expression body = BindNode(node, parameter, fieldMap);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression BindNode(FilterNode node, ParameterExpression parameter, FieldMap<T> fieldMap)
    {
        return node switch
        {
            BinaryNode binary => BindBinary(binary, parameter, fieldMap),
            UnaryNode unary => BindUnary(unary, parameter, fieldMap),
            CallNode call => BindCall(call, parameter, fieldMap),
            FieldNode field => BindField(field, parameter, fieldMap),
            LiteralNode literal => BindLiteral(literal),
            GroupNode group => BindNode(group.Expression, parameter, fieldMap),
            _ => throw new NotSupportedException($"Unknown node type: {node.GetType().Name}")
        };
    }

    private static Expression BindBinary(BinaryNode node, ParameterExpression parameter, FieldMap<T> fieldMap)
    {
        Expression left = BindNode(node.Left, parameter, fieldMap);
        Expression right = BindNode(node.Right, parameter, fieldMap);

        return node.Operator switch
        {
            FilterOperator.And => Expression.AndAlso(left, right),
            FilterOperator.Or => Expression.OrElse(left, right),
            FilterOperator.Equal => BuildEqual(left, right),
            FilterOperator.NotEqual => BuildNotEqual(left, right),
            FilterOperator.GreaterThan => Expression.GreaterThan(left, ConvertIfNeeded(right, left.Type)),
            FilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, ConvertIfNeeded(right, left.Type)),
            FilterOperator.LessThan => Expression.LessThan(left, ConvertIfNeeded(right, left.Type)),
            FilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(left, ConvertIfNeeded(right, left.Type)),
            _ => throw new NotSupportedException($"Binary operator {node.Operator} not supported")
        };
    }

    private static Expression BindUnary(UnaryNode node, ParameterExpression parameter, FieldMap<T> fieldMap)
    {
        Expression operand = BindNode(node.Operand, parameter, fieldMap);

        return node.Operator switch
        {
            FilterOperator.Not => Expression.Not(operand),
            FilterOperator.IsNull => BuildIsNull(operand),
            FilterOperator.NotNull => BuildIsNotNull(operand),
            _ => throw new NotSupportedException($"Unary operator {node.Operator} not supported")
        };
    }

    private static Expression BindCall(CallNode node, ParameterExpression parameter, FieldMap<T> fieldMap)
    {
        Expression target = BindNode(node.Target, parameter, fieldMap);
        List<Expression> args = node.Arguments.Select(a => BindNode(a, parameter, fieldMap)).ToList();

        return node.Operator switch
        {
            FilterOperator.Contains => BuildStringMethod(target, args[0], nameof(string.Contains)),
            FilterOperator.NotContains => Expression.Not(BuildStringMethod(target, args[0], nameof(string.Contains))),
            FilterOperator.StartsWith => BuildStringMethod(target, args[0], nameof(string.StartsWith)),
            FilterOperator.EndsWith => BuildStringMethod(target, args[0], nameof(string.EndsWith)),
            FilterOperator.In => BuildIn(target, args),
            FilterOperator.NotIn => Expression.Not(BuildIn(target, args)),
            FilterOperator.Between => BuildBetween(target, args[0], args[1]),
            FilterOperator.ContainsAny => BuildContainsAny(target, args),
            _ => throw new NotSupportedException($"Call operator {node.Operator} not supported")
        };
    }

    private static Expression BindField(FieldNode node, ParameterExpression parameter, FieldMap<T> fieldMap)
    {
        if (!fieldMap.TryGet(node.Name, out LambdaExpression expression, out _))
        {
            throw new InvalidOperationException($"Field '{node.Name}' not found in field map");
        }

        // Replace parameter in expression
        ParameterReplacerVisitor visitor = new(expression.Parameters[0], parameter);
        return visitor.Visit(expression.Body);
    }

    private static Expression BindLiteral(LiteralNode node)
    {
        return Expression.Constant(node.Value, node.Type);
    }

    // Helper methods

    private static Expression BuildEqual(Expression left, Expression right)
    {
        right = ConvertIfNeeded(right, left.Type);

        // Handle nullable types
        if (IsNullableType(left.Type) && right is ConstantExpression { Value: null })
        {
            return Expression.Equal(left, Expression.Constant(null, left.Type));
        }

        return Expression.Equal(left, right);
    }

    private static Expression BuildNotEqual(Expression left, Expression right)
    {
        right = ConvertIfNeeded(right, left.Type);
        return Expression.NotEqual(left, right);
    }

    private static Expression BuildIsNull(Expression operand)
    {
        return Expression.Equal(operand, Expression.Constant(null, operand.Type));
    }

    private static Expression BuildIsNotNull(Expression operand)
    {
        return Expression.NotEqual(operand, Expression.Constant(null, operand.Type));
    }

    private static Expression BuildStringMethod(Expression target, Expression arg, string methodName)
    {
        // Handle null strings
        BinaryExpression nullCheck = Expression.NotEqual(target, Expression.Constant(null, target.Type));

        MethodInfo method = typeof(string).GetMethod(methodName, new[] { typeof(string) })
                            ?? throw new InvalidOperationException($"Method {methodName} not found on string");

        MethodCallExpression call = Expression.Call(target, method, ConvertIfNeeded(arg, typeof(string)));

        return Expression.AndAlso(nullCheck, call);
    }

    private static Expression BuildIn(Expression target, List<Expression> values)
    {
        if (values.Count == 0)
        {
            return Expression.Constant(false);
        }

        Type targetType = target.Type;
        List<Expression> convertedValues = values.Select(v => ConvertIfNeeded(v, targetType)).ToList();

        // Build: target == value1 || target == value2 || ...
        Expression condition = BuildEqual(target, convertedValues[0]);
        for (int i = 1; i < convertedValues.Count; i++)
        {
            condition = Expression.OrElse(condition, BuildEqual(target, convertedValues[i]));
        }

        return condition;
    }

    private static Expression BuildBetween(Expression target, Expression min, Expression max)
    {
        Type targetType = target.Type;
        min = ConvertIfNeeded(min, targetType);
        max = ConvertIfNeeded(max, targetType);

        BinaryExpression gte = Expression.GreaterThanOrEqual(target, min);
        BinaryExpression lte = Expression.LessThanOrEqual(target, max);

        return Expression.AndAlso(gte, lte);
    }

    private static Expression BuildContainsAny(Expression target, List<Expression> values)
    {
        // For string fields with string array: field @containsAny ('a', 'b')
        // means: field.Contains('a') || field.Contains('b')
        if (target.Type == typeof(string))
        {
            if (values.Count == 0)
            {
                return Expression.Constant(false);
            }

            Expression condition = BuildStringMethod(target, values[0], nameof(string.Contains));
            for (int i = 1; i < values.Count; i++)
            {
                condition = Expression.OrElse(condition, BuildStringMethod(target, values[i], nameof(string.Contains)));
            }

            return condition;
        }

        throw new NotSupportedException($"ContainsAny not supported for type {target.Type}");
    }

    private static Expression ConvertIfNeeded(Expression expression, Type targetType)
    {
        if (expression.Type == targetType)
        {
            return expression;
        }

        // Handle nullable types
        Type underlyingTargetType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        Type underlyingSourceType = Nullable.GetUnderlyingType(expression.Type) ?? expression.Type;

        if (expression is ConstantExpression constant)
        {
            if (constant.Value == null)
            {
                return Expression.Constant(null, targetType);
            }

            // Convert constant value
            object convertedValue = Convert.ChangeType(constant.Value, underlyingTargetType,
                CultureInfo.InvariantCulture);
            return Expression.Constant(convertedValue, targetType);
        }

        return Expression.Convert(expression, targetType);
    }

    private static bool IsNullableType(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
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
