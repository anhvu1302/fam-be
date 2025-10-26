namespace FAM.Application.Companies.DTOs;

/// <summary>
/// Company Data Transfer Object
/// </summary>
public class CompanyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
}