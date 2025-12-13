namespace FAM.Application.Settings;

/// <summary>
/// Backend API configuration options
/// </summary>
public class BackendOptions
{
    public const string SectionName = "Backend";

    /// <summary>
    /// Base URL của backend API
    /// Development: http://localhost:8000 (with port)
    /// Production: https://api.fam.yourdomain.com (no port, uses default 443)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Port for development environment (optional, overrides port in BaseUrl)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Get full backend URL with proper port handling
    /// </summary>
    public string GetBaseUrl()
    {
        var url = BaseUrl.TrimEnd('/');

        // If Port is specified and URL doesn't already have a port, add it
        var schemeEndIndex = url.IndexOf("://", StringComparison.Ordinal);
        if (Port.HasValue && schemeEndIndex >= 0 && !url.Substring(schemeEndIndex + 3).Contains(':'))
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}:{Port.Value}";
        }

        return url;
    }

    /// <summary>
    /// Get full API URL with path
    /// </summary>
    public string GetApiUrl(string path)
    {
        return $"{GetBaseUrl()}/{path.TrimStart('/')}";
    }
}

/// <summary>
/// Frontend configuration options
/// </summary>
public class FrontendOptions
{
    public const string SectionName = "Frontend";

    /// <summary>
    /// Base URL của frontend
    /// Development: http://localhost:3000 (with port)
    /// Production: https://fam.yourdomain.com (no port, uses default 443)
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Port for development environment (optional, overrides port in BaseUrl)
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// Path cho trang reset password (e.g., /reset-password)
    /// </summary>
    public string ResetPasswordPath { get; set; } = "/reset-password";

    /// <summary>
    /// Path cho trang verify email (e.g., /verify-email)
    /// </summary>
    public string VerifyEmailPath { get; set; } = "/verify-email";

    /// <summary>
    /// Path cho trang login (e.g., /login)
    /// </summary>
    public string LoginPath { get; set; } = "/login";

    /// <summary>
    /// Thời gian hết hạn của reset password token (tính bằng phút)
    /// Default: 15 phút
    /// </summary>
    public int PasswordResetTokenExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Get full frontend URL with proper port handling
    /// </summary>
    private string GetBaseUrl()
    {
        var url = BaseUrl.TrimEnd('/');

        // If Port is specified and URL doesn't already have a port, add it
        var schemeEndIndex = url.IndexOf("://", StringComparison.Ordinal);
        if (Port.HasValue && schemeEndIndex >= 0 && !url.Substring(schemeEndIndex + 3).Contains(':'))
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}:{Port.Value}";
        }

        return url;
    }

    /// <summary>
    /// Get full reset password URL
    /// </summary>
    public string GetResetPasswordUrl()
    {
        return $"{GetBaseUrl()}{ResetPasswordPath}";
    }

    /// <summary>
    /// Get full verify email URL
    /// </summary>
    public string GetVerifyEmailUrl()
    {
        return $"{GetBaseUrl()}{VerifyEmailPath}";
    }

    /// <summary>
    /// Get full login URL
    /// </summary>
    public string GetLoginUrl()
    {
        return $"{GetBaseUrl()}{LoginPath}";
    }
}
