namespace FAM.Infrastructure.Services.Dtos;

public class OtpData
{
    public string Code { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string SessionTokenHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
