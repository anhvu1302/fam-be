using FAM.Application.Abstractions;
using FAM.Application.Storage.Commands;
using FAM.Domain.Abstractions;
using FAM.Domain.Abstractions.Repositories;
using FAM.Domain.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FAM.Application.Storage.Handlers;

/// <summary>
/// Handler for initializing upload session
/// Creates session in DB and returns presigned URL for direct upload to MinIO
/// </summary>
public class InitUploadSessionHandler : IRequestHandler<InitUploadSessionCommand, InitUploadSessionResponse>
{
    private readonly IUploadSessionRepository _sessionRepository;
    private readonly IStorageService _storageService;
    private readonly IFileValidator _fileValidator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InitUploadSessionHandler> _logger;

    public InitUploadSessionHandler(
        IUploadSessionRepository sessionRepository,
        IStorageService storageService,
        IFileValidator fileValidator,
        IUnitOfWork unitOfWork,
        ILogger<InitUploadSessionHandler> logger)
    {
        _sessionRepository = sessionRepository;
        _storageService = storageService;
        _fileValidator = fileValidator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InitUploadSessionResponse> Handle(
        InitUploadSessionCommand request,
        CancellationToken cancellationToken)
    {
        // Check idempotency
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existingSession = await _sessionRepository.GetByIdempotencyKeyAsync(
                request.IdempotencyKey,
                cancellationToken);

            if (existingSession != null)
            {
                _logger.LogInformation(
                    "Returning existing session for idempotency key {IdempotencyKey}",
                    request.IdempotencyKey);

                return await GenerateResponseFromSession(existingSession, cancellationToken);
            }
        }

        // Validate file
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            request.FileName,
            request.FileSize);

        if (!isValid || !fileType.HasValue)
            throw new InvalidOperationException(errorMessage ?? "File validation failed");

        // Generate upload ID and temp key
        var uploadId = Guid.NewGuid().ToString("N");
        var tempKey = $"tmp/{uploadId}";

        // Create session
        var session = UploadSession.Create(
            uploadId,
            tempKey,
            request.FileName,
            fileType.Value,
            request.FileSize,
            request.ContentType,
            (int)request.UserId,
            24,
            request.IdempotencyKey);

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created upload session {UploadId} for file {FileName}",
            uploadId,
            request.FileName);

        return await GenerateResponseFromSession(session, cancellationToken);
    }

    private async Task<InitUploadSessionResponse> GenerateResponseFromSession(
        UploadSession session,
        CancellationToken cancellationToken)
    {
        // Generate presigned PUT URL (1 hour expiry for upload)
        var presignedUrl = await _storageService.GetPresignedPutUrlAsync(
            session.TempKey,
            session.ContentType,
            3600, // 1 hour
            cancellationToken);

        return new InitUploadSessionResponse
        {
            UploadId = session.UploadId,
            TempKey = session.TempKey,
            PresignedPutUrl = presignedUrl,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            FileType = session.FileType
        };
    }
}