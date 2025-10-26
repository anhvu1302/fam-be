using FAM.Application.Companies.DTOs;
using MediatR;

namespace FAM.Application.Companies.Commands;

/// <summary>
/// Command to update an existing company
/// </summary>
public class UpdateCompanyCommand : IRequest<CompanyDto>
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public long? UpdatedBy { get; set; }
}
