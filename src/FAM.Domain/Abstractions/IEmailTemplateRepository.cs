using FAM.Domain.EmailTemplates;

namespace FAM.Domain.Abstractions;

/// <summary>
/// Repository interface for EmailTemplate
/// </summary>
public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    /// <summary>
    /// Get template by code
    /// </summary>
    Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active templates
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get templates by category
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetByCategoryAsync(
        EmailTemplateCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if code exists
    /// </summary>
    Task<bool> CodeExistsAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default);
}