using FAM.Application.Companies.DTOs;
using MediatR;

namespace FAM.Application.Companies.Commands;

/// <summary>
/// Command to create a new company
/// </summary>
public class CreateCompanyCommand : IRequest<CompanyDto>
{
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public long? CreatedBy { get; set; }
}