using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users.Entities;
using MediatR;

namespace FAM.Application.Users.Commands.UpdateUserTheme;

public class UpdateUserThemeCommandHandler : IRequestHandler<UpdateUserThemeCommand, UpdateUserThemeResponse>
{
    private readonly IUserThemeRepository _userThemeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserThemeCommandHandler(
        IUserThemeRepository userThemeRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userThemeRepository = userThemeRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateUserThemeResponse> Handle(UpdateUserThemeCommand request,
        CancellationToken cancellationToken)
    {
        // Verify user exists
        var userExists = await _userRepository.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists) throw new DomainException(ErrorCodes.USER_NOT_FOUND, "User not found.");

        // Get or create theme
        var theme = await _userThemeRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (theme == null)
        {
            // Create new theme
            theme = UserTheme.Create(
                request.UserId,
                request.Theme,
                request.PrimaryColor,
                request.Transparency,
                request.BorderRadius,
                request.DarkTheme,
                request.PinNavbar,
                request.CompactMode
            );
            await _userThemeRepository.AddAsync(theme, cancellationToken);
        }
        else
        {
            // Update existing theme
            theme.UpdateTheme(
                request.Theme,
                request.PrimaryColor,
                request.Transparency,
                request.BorderRadius,
                request.DarkTheme,
                request.PinNavbar,
                request.CompactMode
            );
            _userThemeRepository.Update(theme);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateUserThemeResponse(
            theme.Id,
            theme.UserId,
            theme.Theme,
            theme.PrimaryColor,
            theme.Transparency,
            theme.BorderRadius,
            theme.DarkTheme,
            theme.PinNavbar,
            theme.CompactMode
        );
    }
}