using MediatR;

namespace FAM.Application.Menu.Commands.UpdateMenuSortOrders;

public record UpdateMenuSortOrdersCommand(Dictionary<long, int> SortOrders) : IRequest;
