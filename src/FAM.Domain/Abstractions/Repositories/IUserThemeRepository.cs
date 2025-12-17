using FAM.Domain.Users.Entities;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface for UserTheme aggregate
/// </summary>
public interface IUserThemeRepository : IRepository<UserTheme>
{
    Task<UserTheme?> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}
