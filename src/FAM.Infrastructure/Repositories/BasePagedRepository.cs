using System.Linq.Expressions;
using AutoMapper;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// Base repository with common helper methods for expression conversion
/// Provider-agnostic base class for both PostgreSQL and MongoDB
/// </summary>
public abstract class BasePagedRepository<TDomain, TEf>
    where TDomain : class
    where TEf : class
{
    protected readonly IMapper Mapper;

    protected BasePagedRepository(IMapper mapper)
    {
        Mapper = mapper;
    }

    /// <summary>
    /// Convert domain filter expression to EF expression
    /// This allows filtering at database level instead of in-memory
    /// </summary>
    protected Expression<Func<TEf, bool>> ConvertToEfExpression(
        Expression<Func<TDomain, bool>> domainExpression)
    {
        var parameter = Expression.Parameter(typeof(TEf), "e");
        var visitor = new DomainToEfExpressionVisitor(parameter, domainExpression.Parameters[0]);
        var body = visitor.Visit(domainExpression.Body);
        return Expression.Lambda<Func<TEf, bool>>(body, parameter);
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
        if (expression is MemberExpression memberExpression) return memberExpression.Member.Name;

        throw new InvalidOperationException($"Cannot extract property name from expression: {expression}");
    }

    /// <summary>
    /// Parse include string and return corresponding expressions with validation
    /// </summary>
    protected static Expression<Func<TDomain, object>>[] ParseIncludes(
        string? includeString,
        Dictionary<string, Expression<Func<TDomain, object>>> allowedIncludes)
    {
        if (string.IsNullOrWhiteSpace(includeString))
            return Array.Empty<Expression<Func<TDomain, object>>>();

        var includeNames = includeString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        var expressions = new List<Expression<Func<TDomain, object>>>();

        foreach (var includeName in includeNames)
            if (allowedIncludes.TryGetValue(includeName, out var expression))
                expressions.Add(expression);
            else
                throw new InvalidOperationException(
                    $"Include '{includeName}' is not allowed. Allowed includes: {string.Join(", ", allowedIncludes.Keys)}");

        return expressions.ToArray();
    }


    /// <summary>
    /// Expression visitor to convert domain expressions to EF expressions
    /// This enables database-level filtering with domain entity expressions
    /// </summary>
    protected class DomainToEfExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _efParameter;
        private readonly ParameterExpression _domainParameter;

        public DomainToEfExpressionVisitor(
            ParameterExpression efParameter,
            ParameterExpression domainParameter)
        {
            _efParameter = efParameter;
            _domainParameter = domainParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _domainParameter ? _efParameter : base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Handle nested property access like p.Resource.Value -> p.Resource
            if (node.Expression is MemberExpression innerMember)
                // Check if innerMember is accessing domain parameter
                if (innerMember.Expression is ParameterExpression paramExpr && paramExpr == _domainParameter)
                {
                    // This is accessing a property on a ValueObject (e.g., p.Resource.Value)
                    // We want to map this to the EF property (e.g., p.Resource)
                    var efProperty = typeof(TEf).GetProperty(innerMember.Member.Name);
                    if (efProperty != null)
                        // Return just the EF property, dropping the .Value access
                        return Expression.Property(_efParameter, efProperty);
                }

            // If member is accessed directly on domain parameter, convert to EF parameter
            if (node.Expression is ParameterExpression directParamExpr && directParamExpr == _domainParameter)
            {
                // Get the same property name on EF type
                var efProperty = typeof(TEf).GetProperty(node.Member.Name);
                if (efProperty != null) return Expression.Property(_efParameter, efProperty);
            }

            return base.VisitMember(node);
        }
    }
}