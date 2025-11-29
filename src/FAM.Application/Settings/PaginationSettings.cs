namespace FAM.Application.Settings;

/// <summary>
/// Configuration settings for pagination across the application.
/// Can be configured in appsettings.json under "Pagination" section.
/// </summary>
public class PaginationSettings
{
    public const string SectionName = "Pagination";

    /// <summary>
    /// Default page number when not specified. Default: 1
    /// </summary>
    public int DefaultPage { get; set; } = 1;

    /// <summary>
    /// Default number of items per page. Default: 10
    /// </summary>
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    /// Maximum allowed page size to prevent performance issues. Default: 100
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Normalize page size to be within valid bounds
    /// </summary>
    public int NormalizePageSize(int? requestedSize)
    {
        if (!requestedSize.HasValue || requestedSize.Value <= 0)
            return DefaultPageSize;

        return Math.Min(requestedSize.Value, MaxPageSize);
    }

    /// <summary>
    /// Normalize page number to be at least 1
    /// </summary>
    public int NormalizePage(int? requestedPage)
    {
        if (!requestedPage.HasValue || requestedPage.Value <= 0)
            return DefaultPage;

        return requestedPage.Value;
    }
}
