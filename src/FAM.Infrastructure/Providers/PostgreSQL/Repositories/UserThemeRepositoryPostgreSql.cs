using AutoMapper;
using FAM.Domain.Abstractions;
using FAM.Domain.Users.Entities;
using FAM.Infrastructure.PersistenceModels.Ef;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Providers.PostgreSQL.Repositories;

/// <summary>
/// PostgreSQL implementation of IUserThemeRepository
/// </summary>
public class UserThemeRepositoryPostgreSql : IUserThemeRepository
{
    private readonly PostgreSqlDbContext _context;
    private readonly IMapper _mapper;

    public UserThemeRepositoryPostgreSql(PostgreSqlDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserTheme?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserThemes.FindAsync(new object[] { id }, cancellationToken);
        return entity != null ? _mapper.Map<UserTheme>(entity) : null;
    }

    public async Task<UserTheme?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserThemes
            .FirstOrDefaultAsync(ut => ut.UserId == userId && !ut.IsDeleted, cancellationToken);
        return entity != null ? _mapper.Map<UserTheme>(entity) : null;
    }

    public async Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserThemes
            .AnyAsync(ut => ut.UserId == userId && !ut.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<UserTheme>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.UserThemes.ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserTheme>>(entities);
    }

    public async Task<IEnumerable<UserTheme>> FindAsync(System.Linq.Expressions.Expression<Func<UserTheme, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var allEntities = await _context.UserThemes.ToListAsync(cancellationToken);
        var allThemes = _mapper.Map<IEnumerable<UserTheme>>(allEntities);
        return allThemes.Where(predicate.Compile());
    }

    public async Task AddAsync(UserTheme entity, CancellationToken cancellationToken = default)
    {
        var efEntity = _mapper.Map<UserThemeEf>(entity);
        await _context.UserThemes.AddAsync(efEntity, cancellationToken);
    }

    public void Update(UserTheme entity)
    {
        var efEntity = _mapper.Map<UserThemeEf>(entity);
        var existingEntry = _context.ChangeTracker.Entries<UserThemeEf>()
            .FirstOrDefault(e => e.Entity.Id == efEntity.Id);

        if (existingEntry != null)
        {
            _context.Entry(existingEntry.Entity).CurrentValues.SetValues(efEntity);
            existingEntry.State = EntityState.Modified;
        }
        else
        {
            _context.UserThemes.Attach(efEntity);
            _context.Entry(efEntity).State = EntityState.Modified;
        }
    }

    public void Delete(UserTheme entity)
    {
        var efEntity = _mapper.Map<UserThemeEf>(entity);
        _context.UserThemes.Remove(efEntity);
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.UserThemes.AnyAsync(ut => ut.Id == id && !ut.IsDeleted, cancellationToken);
    }
}
