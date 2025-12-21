using System.Linq.Expressions;

using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using FAM.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IEmailTemplateRepository
/// Uses Pragmatic Architecture - directly works with Domain entities
/// </summary>
public class EmailTemplateRepository : BaseRepository<EmailTemplate>,
    IEmailTemplateRepository
{
    public EmailTemplateRepository(PostgreSqlDbContext context)
        : base(context)
    {
    }


    public async Task<EmailTemplate?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmailTemplate>> FindAsync(
        Expression<Func<EmailTemplate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        // Apply predicate at database level - no need for in-memory filtering
        return await Context.EmailTemplates.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        await Context.EmailTemplates.AddAsync(entity, cancellationToken);
    }

    public void Update(EmailTemplate entity)
    {
        var trackedEntry = Context.ChangeTracker.Entries<EmailTemplate>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            Context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            Context.EmailTemplates.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(EmailTemplate entity)
    {
        Context.EmailTemplates.Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates
            .FirstOrDefaultAsync(e => e.Code == code.ToUpper(), cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByCategoryAsync(
        EmailTemplateCategory category,
        CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates
            .Where(e => e.Category == category)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EmailTemplate> query = Context.EmailTemplates.Where(e => e.Code == code.ToUpper());

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

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
        IQueryable<EmailTemplate> countQuery = Context.EmailTemplates.AsQueryable();
        IQueryable<EmailTemplate> dataQuery = Context.EmailTemplates.AsQueryable();

        // Apply includes if provided
        if (includes != null && includes.Any())
        {
            foreach (var include in includes)
            {
                var propertyPath = GetPropertyName(include.Body);
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
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting
        dataQuery = ApplySort(dataQuery, sort, GetSortExpression, t => t.Id);

        // Apply pagination and execute
        var templates = await dataQuery
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
