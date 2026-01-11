using FAM.Application.Common.Helpers;
using FAM.Application.Users.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Users;

using MediatR;

namespace FAM.Application.Users.Queries.GetUserById;

/// <summary>
/// Handler for GetUserByIdQuery - Uses Repository directly
/// </summary>
public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return null;
        }

        HashSet<string> includeSet = IncludeParser.Parse(request.Include);
        return user.ToUserDto(includeSet);
    }
}
