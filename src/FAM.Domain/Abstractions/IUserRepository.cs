using FAM.Domain.Users;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface cho User aggregate
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameTakenAsync(string username, long? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> IsEmailTakenAsync(string email, long? excludeUserId = null, CancellationToken cancellationToken = default);
}