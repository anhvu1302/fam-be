using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using FAM.Infrastructure.Common.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL implementation of IEmailTemplateRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// Follows Clean Architecture by depending on IDbContext
/// </summary>
public class EmailTemplateRepository : BaseRepository<EmailTemplate>,
    IEmailTemplateRepository
{
    public EmailTemplateRepository(IDbContext context)
        : base(context)
    {
    }


    public async Task<EmailTemplate?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> FindAsync(
        Expression<Func<EmailTemplate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(EmailTemplate entity)
    {
        EntityEntry<EmailTemplate>? trackedEntry = Context.ChangeTracker.Entries<EmailTemplate>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            DbSet.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(EmailTemplate entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.Code == code.ToUpper(), cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByCategoryAsync(
        EmailTemplateCategory category,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EmailTemplate> query = DbSet.Where(e => e.Code == code.ToUpper());

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<(IEnumerable<EmailTemplate> Items, long Total)> GetPagedAsync(
        Expression<Func<EmailTemplate, bool>>? filter = null,
        string? sort = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Expression<Func<EmailTemplate, object>>>? includes = null,
        CancellationToken cancellationToken = default)
    {
        // Build base query
        IQueryable<EmailTemplate> countQuery = DbSet.AsQueryable();
        IQueryable<EmailTemplate> dataQuery = DbSet.AsQueryable();

        // Apply includes if provided
        if (includes != null && includes.Any())
        {
            foreach (Expression<Func<EmailTemplate, object>> include in includes)
            {
                string propertyPath = GetPropertyName(include.Body);
                dataQuery = dataQuery.Include(propertyPath);
            }
        }

        // Apply filter at database level
        if (filter != null)
        {
            countQuery = countQuery.Where(filter);
            dataQuery = dataQuery.Where(filter);
        }

        // Get total count
        long total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, t => t.Id);

        // Apply pagination and execute
        List<EmailTemplate> templates = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (templates, total);
    }

    private Expression<Func<EmailTemplate, object>> GetSortExpression(string fieldName)
    {
        return fieldName switch
        {
            "id" => t => t.Id,
            "code" => t => t.Code,
            "name" => t => t.Name,
            "subject" => t => t.Subject,
            "category" => t => t.Category,
            "isactive" => t => t.IsActive,
            "issystem" => t => t.IsSystem,
            "createdat" => t => t.CreatedAt,
            "updatedat" => t => t.UpdatedAt ?? DateTime.MinValue,
            _ => throw new InvalidOperationException($"Field '{fieldName}' cannot be used for sorting")
        };
    }
}
