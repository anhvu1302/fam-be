namespace FAM.Domain.Abstractions;

/// <summary>
/// Specification pattern - encapsulate query logic
/// </summary>
public interface ISpecification<T>
{
    System.Linq.Expressions.Expression<Func<T, bool>> ToExpression();
    bool IsSatisfiedBy(T entity);
}

/// <summary>
/// Base class cho specifications
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract System.Linq.Expressions.Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
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

    public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
        var body = System.Linq.Expressions.Expression.AndAlso(
            System.Linq.Expressions.Expression.Invoke(leftExpr, param),
            System.Linq.Expressions.Expression.Invoke(rightExpr, param)
        );
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
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

    public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();
        var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
        var body = System.Linq.Expressions.Expression.OrElse(
            System.Linq.Expressions.Expression.Invoke(leftExpr, param),
            System.Linq.Expressions.Expression.Invoke(rightExpr, param)
        );
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
    }
}

internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _spec;

    public NotSpecification(Specification<T> spec)
    {
        _spec = spec;
    }

    public override System.Linq.Expressions.Expression<Func<T, bool>> ToExpression()
    {
        var expr = _spec.ToExpression();
        var param = System.Linq.Expressions.Expression.Parameter(typeof(T));
        var body = System.Linq.Expressions.Expression.Not(
            System.Linq.Expressions.Expression.Invoke(expr, param)
        );
        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
    }
}
