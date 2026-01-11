using System.Linq.Expressions;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Specification pattern - encapsulate query logic
/// </summary>
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

/// <summary>
/// Base class cho specifications
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        Func<T, bool> predicate = ToExpression().Compile();
        return predicate(entity);
    }

    // Combinator methods
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}

internal class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = _left.ToExpression();
        Expression<Func<T, bool>> rightExpr = _right.ToExpression();
        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.AndAlso(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> leftExpr = _left.ToExpression();
        Expression<Func<T, bool>> rightExpr = _right.ToExpression();
        ParameterExpression param = Expression.Parameter(typeof(T));
        BinaryExpression body = Expression.OrElse(
            Expression.Invoke(leftExpr, param),
            Expression.Invoke(rightExpr, param)
        );
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _spec;

    public NotSpecification(Specification<T> spec)
    {
        _spec = spec;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        Expression<Func<T, bool>> expr = _spec.ToExpression();
        ParameterExpression param = Expression.Parameter(typeof(T));
        UnaryExpression body = Expression.Not(
            Expression.Invoke(expr, param)
        );
        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
