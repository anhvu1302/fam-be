using System.Linq.Expressions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Domain.Storage;
using FAM.Infrastructure.Providers.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories;

/// <summary>
/// PostgreSQL repository for UploadSession
/// </summary>
public class UploadSessionRepository : IUploadSessionRepository
{
    private readonly PostgreSqlDbContext _context;

    public UploadSessionRepository(PostgreSqlDbContext context)
    {
        _context = context;
    }

    public async Task<UploadSession?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>().FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<UploadSession>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UploadSession>> FindAsync(
        Expression<Func<UploadSession, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UploadSession entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<UploadSession>().AddAsync(entity, cancellationToken);
    }

    public void Update(UploadSession entity)
    {
        var trackedEntry = _context.ChangeTracker.Entries<UploadSession>()
            .FirstOrDefault(e => e.Entity.Id == entity.Id);

        if (trackedEntry != null)
        {
            _context.Entry(trackedEntry.Entity).CurrentValues.SetValues(entity);
        }
        else
        {
            _context.Set<UploadSession>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }

    public void Delete(UploadSession entity)
    {
        _context.Set<UploadSession>().Remove(entity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>().AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<UploadSession?> GetByUploadIdAsync(
        string uploadId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>()
            .FirstOrDefaultAsync(s => s.UploadId == uploadId, cancellationToken);
    }

    public async Task<IReadOnlyList<UploadSession>> GetSessionsForCleanupAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.Set<UploadSession>()
            .Where(s =>
                s.Status == UploadSessionStatus.Expired ||
                s.Status == UploadSessionStatus.Failed ||
                (s.Status == UploadSessionStatus.Pending && s.ExpiresAt < now) ||
                (s.Status == UploadSessionStatus.Uploaded && s.ExpiresAt < now))
            .OrderBy(s => s.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UploadSession>> GetByEntityAsync(
        int entityId,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>()
            .Where(s => s.EntityId == entityId && s.EntityType == entityType)
            .ToListAsync(cancellationToken);
    }

    public async Task<UploadSession?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UploadSession>()
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey, cancellationToken);
    }
}