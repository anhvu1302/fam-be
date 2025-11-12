using FAM.Application.Abstractions;
using FAM.Application.Users.Commands;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Domain.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for updating user avatar using upload session pattern
/// Implements "Finalize" step: move file from tmp/ to users/ and link to user
/// </summary>
public class UpdateAvatarHandler : IRequestHandler<UpdateAvatarCommand, UpdateAvatarResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IUploadSessionRepository _sessionRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAvatarHandler> _logger;

    public UpdateAvatarHandler(
        IUserRepository userRepository,
        IUploadSessionRepository sessionRepository,
        IStorageService storageService,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAvatarHandler> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateAvatarResponse> Handle(
        UpdateAvatarCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate upload session
        var session = await _sessionRepository.GetByUploadIdAsync(request.UploadId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Upload session {request.UploadId} not found");
        }

        if (session.Status != UploadSessionStatus.Pending && session.Status != UploadSessionStatus.Uploaded)
        {
            throw new InvalidOperationException($"Upload session is in {session.Status} status, cannot finalize");
        }

        if (session.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("Upload session does not belong to this user");
        }

        // 2. Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        // Start transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 3. Move file from tmp/ to users/{userId}/
            var finalKey = $"users/{request.UserId}/avatar-{DateTime.UtcNow:yyyyMMdd-HHmmss}{Path.GetExtension(session.FileName)}";
            
            await _storageService.MoveObjectAsync(session.TempKey, finalKey, cancellationToken);

            // 4. Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                try
                {
                    await _storageService.DeleteFileAsync(user.Avatar, cancellationToken);
                    _logger.LogInformation("Deleted old avatar {OldAvatar} for user {UserId}", user.Avatar, request.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old avatar {OldAvatar}, continuing", user.Avatar);
                    // Don't fail the whole operation if old avatar deletion fails
                }
            }

            // 5. Update user avatar
            user.UpdatePersonalInfo(
                user.FirstName,
                user.LastName,
                finalKey,
                user.Bio,
                user.DateOfBirth);

            _userRepository.Update(user);

            // 6. Finalize upload session
            session.Finalize(finalKey, (int)request.UserId, "User", checksum: null);
            _sessionRepository.Update(session);

            // 7. Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 8. Generate presigned URL for the new avatar (1 hour expiry)
            var avatarUrl = await _storageService.GetPresignedUrlAsync(finalKey, 3600);

            _logger.LogInformation(
                "Updated avatar for user {UserId} from upload session {UploadId}",
                request.UserId,
                request.UploadId);

            return new UpdateAvatarResponse
            {
                AvatarUrl = avatarUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(
                ex,
                "Failed to update avatar for user {UserId} from upload session {UploadId}",
                request.UserId,
                request.UploadId);
            throw;
        }
    }
}
