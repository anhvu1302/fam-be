using System.Linq.Expressions;
using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.EmailTemplates;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

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
        var entity = await Context.EmailTemplates.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? Mapper.Map<EmailTemplate>(entity) : null;
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await Context.EmailTemplates.ToListAsync(cancellationToken);
        return Mapper.Map<IEnumerable<EmailTemplate>>(entities);
    }

    public async Task<IEnumerable<EmailTemplate>> FindAsync(
        Expression<Func<EmailTemplate, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await Context.EmailTemplates.ToListAsync(cancellationToken);
        var allTemplates = Mapper.Map<IEnumerable<EmailTemplate>>(allEntities);
        return allTemplates.Where(predicate.Compile());
    }

    public async Task AddAsync(EmailTemplate entity, CancellationToken cancellationToken = default)
    {
        var efEntity = Mapper.Map<EmailTemplateEf>(entity);
        await Context.EmailTemplates.AddAsync(efEntity, cancellationToken);
    }

    public void Update(EmailTemplate entity)
    {
        var efEntity = Mapper.Map<EmailTemplateEf>(entity);

        // Check if the entity is already being tracked
        var existingEntry = Context.ChangeTracker.Entries<EmailTemplateEf>()
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
        var efEntity = Mapper.Map<EmailTemplateEf>(entity);
        Context.EmailTemplates.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Context.EmailTemplates.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await Context.EmailTemplates
            .FirstOrDefaultAsync(e => e.Code == code.ToUpper(), cancellationToken);
        return entity != null ? Mapper.Map<EmailTemplate>(entity) : null;
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await Context.EmailTemplates
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);
        return Mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetByCategoryAsync(
        EmailTemplateCategory category,
        CancellationToken cancellationToken = default)
    {
        var entities = await Context.EmailTemplates
            .Where(e => e.Category == (int)category)
            .ToListAsync(cancellationToken);
        return Mapper.Map<List<EmailTemplate>>(entities);
    }

    public async Task<bool> CodeExistsAsync(string code, long? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.EmailTemplates.Where(e => e.Code == code.ToUpper());

        if (excludeId.HasValue)
            query = query.Where(e => e.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}