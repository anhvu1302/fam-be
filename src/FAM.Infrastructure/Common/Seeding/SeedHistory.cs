namespace FAM.Infrastructure.Common.Seeding;

/// <summary>
/// Tracks which seeders have been executed
/// </summary>
public class SeedHistory
{
    public long Id { get; set; }
    public string SeederName { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string ExecutedBy { get; set; } = "System";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan Duration { get; set; }
}