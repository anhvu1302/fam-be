namespace FAM.Infrastructure.Common.Options;

/// <summary>
/// MinIO storage configuration
/// </summary>
public class MinioSettings
{
    public const string SectionName = "Minio";

    /// <summary>
    /// MinIO endpoint (host:port)
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Access key (username)
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key (password)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Use SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; }

    /// <summary>
    /// Default bucket name
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Region (optional)
    /// </summary>
    public string? Region { get; set; }
}
