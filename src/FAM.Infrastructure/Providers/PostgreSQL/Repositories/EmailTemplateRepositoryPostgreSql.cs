using System.Linq.Expressions;

using AutoMapper;

using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using FAM.Infrastructure.PersistenceModels.Ef;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IEmailTemplateRepository
/// </summary>
public class EmailTemplateRepositoryPostgreSql : BasePagedRepository<EmailTemplate, EmailTemplateEf>,
    IEmailTemplateRepository
{
    public EmailTemplateRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
        : base(context, mapper)
    {
    }

    protected override DbSet<EmailTemplateEf> DbSet => Context.EmailTemplates;

    public async Task<EmailTemplate?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        EmailTemplateEf? entity = await Context.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<EmailTemplate>(entity) : null;
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<EmailTemplateEf> entities = await Context.EmailTemplates.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<EmailTemplate>>(entities);
    }

    public async Task<IEnumerable<EmailTemplate>> FindAsync(
        Expression<Func<EmailTemplate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        List<EmailTemplateEf> allEntities = await Context.EmailTemplates.ToListAsync(cancellationToken);
        IEnumerable<EmailTemplate>? allTemplates = Mapper.Map<IEnumerable<EmailTemplate>>(allEntities);
        return allTemplates.Where(predicate.Compile());
    }

    public async Task AddAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        EmailTemplateEf? efEntity = Mapper.Map<EmailTemplateEf>(entity);
        await Context.EmailTemplates.AddAsync(efEntity, cancellationToken);
    }

    public void Update(EmailTemplate entity)
    {
        EmailTemplateEf? efEntity = Mapper.Map<EmailTemplateEf>(entity);

        // Check if the entity is already being tracked
        EntityEntry<EmailTemplateEf>? existingEntry = Context.ChangeTracker.Entries<EmailTemplateEf>()
            .FirstOrDefault(e => e.Entity.Id == efEntity.Id);

        if (existingEntry != null)
        {
            // Entity is already tracked, update its properties
            Context.Entry(existingEntry.Entity).CurrentValues.SetValues(efEntity);
            existingEntry.State = EntityState.Modified;
        }
        else
        {
            // Entity is not tracked, attach and update
            Context.EmailTemplates.Attach(efEntity);
            Context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(EmailTemplate entity)
    {
        EmailTemplateEf? efEntity = Mapper.Map<EmailTemplateEf>(entity);
        Context.EmailTemplates.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        EmailTemplateEf? entity = await Context.EmailTemplates
            .FirstOrDefaultAsync(e => e.Code == code.ToUpper(), cancellationToken);
        return entity != null ? Mapper.Map<EmailTemplate>(entity) : null;
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        List<EmailTemplateEf> entities = await Context.EmailTemplates
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);
        return Mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByCategoryAsync(
        EmailTemplateCategory category,
        CancellationToken cancellationToken = default)
    {
        List<EmailTemplateEf> entities = await Context.EmailTemplates
            .Where(e => e.Category == (int)category)
            .ToListAsync(cancellationToken);
        return Mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<EmailTemplateEf> query = Context.EmailTemplates.Where(e => e.Code == code.ToUpper());

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
        // Build base query for counting
        IQueryable<EmailTemplateEf> countQuery = Context.EmailTemplates.AsQueryable();

        // Build query for data
        IQueryable<EmailTemplateEf> dataQuery = Context.EmailTemplates.AsQueryable();

        // Apply filter at database level
        if (filter != null)
        {
            Expression<Func<EmailTemplateEf, bool>> efFilter = ConvertToEfExpression(filter);
            countQuery = countQuery.Where(efFilter);
            dataQuery = dataQuery.Where(efFilter);
        }

        // Get total count
        var total = await countQuery.LongCountAsync(cancellationToken);

        // Apply sorting - use base class method directly
        if (!string.IsNullOrWhiteSpace(sort))
        {
            var sortParts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
            IOrderedQueryable<EmailTemplateEf>? orderedQuery = null;

            foreach (var sortPart in sortParts)
            {
                var trimmed = sortPart.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                var descending = trimmed.StartsWith('-');
                var fieldName = descending ? trimmed[1..] : trimmed;
                Expression<Func<EmailTemplateEf, object>> sortExpression = GetSortExpression(fieldName);

                orderedQuery = orderedQuery == null
                    ? descending ? dataQuery.OrderByDescending(sortExpression) : dataQuery.OrderBy(sortExpression)
                    : descending
                        ? orderedQuery.ThenByDescending(sortExpression)
                        : orderedQuery.ThenBy(sortExpression);
            }

            dataQuery = orderedQuery ?? dataQuery.OrderBy(t => t.Id);
        }
        else
        {
            dataQuery = dataQuery.OrderBy(t => t.Id);
        }

        // Apply pagination and execute
        List<EmailTemplateEf> entities = await dataQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Map to domain entities
        List<EmailTemplate>? templates = Mapper.Map<List<EmailTemplate>>(entities);

        return (templates, total);
    }

    private Expression<Func<EmailTemplateEf, object>> GetSortExpression(string fieldName)
    {
        return fieldName.ToLowerInvariant() switch
        {
            "id" => t => t.Id,
            "code" => t => t.Code,
            "name" => t => t.Name,
            "subject" => t => t.Subject,
            "category" => t => t.Category,
            "isactive" => t => t.IsActive,
            "issystem" => t => t.IsSystem,
            "createdat" => t => t.CreatedAt,
            "updatedat" => t => t.UpdatedAt!,
            _ => t => t.Id
        };
    }
}
