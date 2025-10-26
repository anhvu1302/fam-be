using MediatR;

namespace FAM.Application.Companies.Commands;

/// <summary>
/// Command to delete a company
/// </summary>
public class DeleteCompanyCommand : IRequest<Unit>
{
    public long Id { get; set; }
    public long? DeletedBy { get; set; }
}