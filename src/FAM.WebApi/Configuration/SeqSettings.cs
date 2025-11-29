namespace FAM.WebApi.Configuration;

/// <summary>
/// Settings cho Seq logging server
/// </summary>
public class SeqSettings
{
    public const string SectionName = "Seq";

    /// <summary>
    /// URL của Seq server (e.g., http://localhost:5341)
    /// </summary>
    public string? ServerUrl { get; set; }

    /// <summary>
    /// API Key để authenticate với Seq (optional)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Có enable Seq sink hay không
    /// </summary>
    public bool Enabled { get; set; } = true;
}