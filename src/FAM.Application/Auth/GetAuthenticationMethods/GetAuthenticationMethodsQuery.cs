using FAM.Application.Auth.Shared;
using MediatR;

namespace FAM.Application.Auth.GetAuthenticationMethods;

/// <summary>
/// Query để lấy thông tin các phương thức xác thực đang bật của user
/// </summary>
public sealed record GetAuthenticationMethodsQuery : IRequest<AuthenticationMethodsResponse>
{
    /// <summary>
    /// User ID cần lấy thông tin
    /// </summary>
    public long UserId { get; init; }
}