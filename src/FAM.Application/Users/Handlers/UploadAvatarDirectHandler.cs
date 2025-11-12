using FAM.Application.Abstractions;
using FAM.Application.Users.Commands;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Domain.Common.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Users.Handlers;

/// <summary>
/// Handler for direct avatar upload with transaction safety
/// Implements atomic upload: storage → DB update, with rollback cleanup
/// </summary>
public class UploadAvatarDirectHandler : IRequestHandler<UploadAvatarDirectCommand, UploadAvatarDirectResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IFileValidator _fileValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadAvatarDirectHandler> _logger;

    public UploadAvatarDirectHandler(
        IUserRepository userRepository,
        IStorageService storageService,
        IFileValidator fileValidator,
        IUnitOfWork unitOfWork,
        ILogger<UploadAvatarDirectHandler> logger)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _fileValidator = fileValidator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UploadAvatarDirectResponse> Handle(
        UploadAvatarDirectCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate file
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            request.FileName,
            request.FileSize);

        if (!isValid || !fileType.HasValue)
        {
            throw new InvalidOperationException(errorMessage);
        }

        // Only allow image files for avatars
        if (fileType.Value != FileType.Image)
        {
            throw new InvalidOperationException("Only image files are allowed for avatar upload");
        }

        // 2. Get user
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User {request.UserId} not found");
        }

        // 3. Generate final file path
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        var finalKey = $"users/{request.UserId}/avatar-{timestamp}{extension}";

        string? uploadedFilePath = null;

        // Start transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 4. Upload file to storage first
            uploadedFilePath = await _storageService.UploadFileAsync(
                request.FileStream,
                finalKey,
                fileType.Value,
                request.ContentType,
                cancellationToken);

            _logger.LogInformation(
                "Uploaded avatar file {FilePath} for user {UserId}",
                uploadedFilePath,
                request.UserId);

            // 5. Delete old avatar if exists
            if (!string.IsNullOrEmpty(user.Avatar))
            {
                try
                {
                    await _storageService.DeleteFileAsync(user.Avatar, cancellationToken);
                    _logger.LogInformation(
                        "Deleted old avatar {OldAvatar} for user {UserId}",
                        user.Avatar,
                        request.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to delete old avatar {OldAvatar}, continuing",
                        user.Avatar);
                    // Don't fail the whole operation if old avatar deletion fails
                }
            }

            // 6. Update user avatar in database
            user.UpdatePersonalInfo(
                user.FirstName,
                user.LastName,
                uploadedFilePath,
                user.Bio,
                user.DateOfBirth);

            _userRepository.Update(user);

            // 7. Commit transaction
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // 8. Generate presigned URL for the new avatar (1 hour expiry)
            var avatarUrl = await _storageService.GetPresignedUrlAsync(uploadedFilePath, 3600);

            _logger.LogInformation(
                "Successfully updated avatar for user {UserId} to {FilePath}",
                request.UserId,
                uploadedFilePath);

            return new UploadAvatarDirectResponse
            {
                AvatarUrl = avatarUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                FilePath = uploadedFilePath
            };
        }
        catch (Exception ex)
        {
            // Rollback transaction
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            // Cleanup: delete uploaded file if it was created
            if (!string.IsNullOrEmpty(uploadedFilePath))
            {
                try
                {
                    await _storageService.DeleteFileAsync(uploadedFilePath, cancellationToken);
                    _logger.LogInformation(
                        "Cleaned up uploaded file {FilePath} after transaction rollback",
                        uploadedFilePath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(
                        cleanupEx,
                        "Failed to cleanup uploaded file {FilePath} after rollback",
                        uploadedFilePath);
                }
            }

            _logger.LogError(
                ex,
                "Failed to upload avatar for user {UserId}",
                request.UserId);

            throw;
        }
    }
}