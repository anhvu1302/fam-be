using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// Base repository with common helper methods for paged queries
/// </summary>
public abstract class BasePagedRepository<TDomain, TEf>
    where TDomain : class
    where TEf : class
{
    protected readonly PostgreSqlDbContext Context;
    protected readonly IMapper Mapper;

    protected BasePagedRepository(PostgreSqlDbContext context, IMapper mapper)
    {
        Context = context;
        Mapper = mapper;
    }

    /// <summary>
    /// Get the DbSet for the entity
    /// </summary>
    protected abstract DbSet<TEf> DbSet { get; }

    /// <summary>
    /// Convert domain filter expression to EF expression
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
    /// Extract property name from expression
    /// </summary>
    protected string GetPropertyName(Expression expression)
    {
        // Handle Convert expression
        if (expression is UnaryExpression unaryExpression &&
            unaryExpression.NodeType == ExpressionType.Convert)
            expression = unaryExpression.Operand;

        // Handle member access
        if (expression is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        throw new InvalidOperationException($"Cannot extract property name from expression: {expression}");
    }

    /// <summary>
    /// Apply sorting to query
    /// </summary>
    protected virtual IQueryable<TEf> ApplySort(IQueryable<TEf> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query;

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var isFirst = true;

        foreach (var field in sortFields)
        {
            var trimmedField = field.Trim();
            var descending = trimmedField.StartsWith("-");
            var propertyName = descending ? trimmedField[1..] : trimmedField;

            // Get property info from EF type
            var property = typeof(TEf).GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public |
                BindingFlags.Instance);

            if (property == null)
                continue;

            var parameter = Expression.Parameter(typeof(TEf), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);

            var methodName = isFirst
                ? descending ? "OrderByDescending" : "OrderBy"
                : descending
                    ? "ThenByDescending"
                    : "ThenBy";

            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEf), property.PropertyType);

            query = (IQueryable<TEf>)method.Invoke(null, new object[] { query, lambda })!;
            isFirst = false;
        }

        return query;
    }

    /// <summary>
    /// Expression visitor to convert domain expressions to EF expressions
    /// </summary>
    protected class DomainToEfExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _domainParameter;
        private readonly ParameterExpression _efParameter;

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
            // If member is accessed on domain parameter, convert to EF parameter
            if (node.Expression is ParameterExpression paramExpr && paramExpr == _domainParameter)
            {
                // Get the same property name on EF type
                var efProperty = typeof(TEf).GetProperty(node.Member.Name);
                if (efProperty != null)
                    return Expression.Property(_efParameter, efProperty);
            }

            return base.VisitMember(node);
        }
    }
}