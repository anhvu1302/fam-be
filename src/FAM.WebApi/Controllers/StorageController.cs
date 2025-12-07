using FAM.Application.Abstractions;
using FAM.Application.Storage.Commands;
using FAM.WebApi.Contracts.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileInfo = FAM.Application.Abstractions.FileInfo;
using InitUploadSessionResponse = FAM.Application.Storage.Commands.InitUploadSessionResponse;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Storage management endpoints (upload, download with time-limited URLs)
/// </summary>
[ApiController]
[Route("api/storage")]
[Authorize]
public class StorageController : BaseApiController
{
    private readonly IStorageService _storageService;
    private readonly IFileValidator _fileValidator;
    private readonly IMediator _mediator;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IStorageService storageService,
        IFileValidator fileValidator,
        IMediator mediator,
        ILogger<StorageController> logger)
    {
        _storageService = storageService;
        _fileValidator = fileValidator;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initialize upload session (Step 1: Get presigned URL for temporary upload)
    /// </summary>
    [HttpPost("sessions/init")]
    [ProducesResponseType(typeof(InitUploadSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitUploadSession([FromBody] InitUploadSessionRequest request)
    {
        try
        {
            // Get userId from claims
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("userId");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                return UnauthorizedResponse("User not authenticated", "USER_NOT_AUTHENTICATED");

            var command = new InitUploadSessionCommand
            {
                FileName = request.FileName,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                UserId = userId,
                IdempotencyKey = request.IdempotencyKey
            };

            var response = await _mediator.Send(command);

            return OkResponse(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse(ex.Message, "INVALID_UPLOAD_SESSION");
        }
        catch (UnauthorizedAccessException ex)
        {
            return UnauthorizedResponse(ex.Message, "UNAUTHORIZED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing upload session for {FileName}", request.FileName);
            return InternalErrorResponse("An error occurred while initializing upload session", "UPLOAD_SESSION_ERROR");
        }
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB max
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequestResponse("File is required", "FILE_REQUIRED");

        // Validate file and auto-detect type
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            file.FileName,
            file.Length);

        if (!isValid || !fileType.HasValue) return BadRequestResponse(errorMessage, "FILE_VALIDATION_FAILED");

        try
        {
            await using var stream = file.OpenReadStream();

            var filePath = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                fileType.Value,
                file.ContentType);

            // Generate presigned URL with 1 hour expiry
            var url = await _storageService.GetPresignedUrlAsync(filePath, 3600);
            var expiresAt = DateTime.UtcNow.AddSeconds(3600);

            return OkResponse(new UploadFileResponse
            {
                FilePath = filePath,
                Url = url,
                ExpiresAt = expiresAt,
                FileSize = file.Length
            }, "File uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return InternalErrorResponse("An error occurred while uploading the file", "FILE_UPLOAD_ERROR");
        }
    }

    /// <summary>
    /// Initiate multipart upload for large files
    /// </summary>
    [HttpPost("multipart/initiate")]
    [ProducesResponseType(typeof(InitiateMultipartUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitiateMultipartUpload(
        [FromBody] InitiateMultipartUploadRequest request)
    {
        // Validate file and auto-detect type
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            request.FileName,
            request.TotalSize);

        if (!isValid || !fileType.HasValue) return BadRequestResponse(errorMessage, "FILE_VALIDATION_FAILED");

        try
        {
            var uploadId = await _storageService.InitiateMultipartUploadAsync(
                request.FileName,
                fileType.Value,
                request.ContentType);

            return OkResponse(new InitiateMultipartUploadResponse
            {
                UploadId = uploadId,
                FilePath = uploadId.Split('_')[0], // Extract object name
                ChunkSize = 5 * 1024 * 1024 // 5 MB recommended chunk size
            }, "Multipart upload initiated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating multipart upload for {FileName}", request.FileName);
            return InternalErrorResponse("An error occurred while initiating multipart upload", "MULTIPART_INIT_ERROR");
        }
    }

    /// <summary>
    /// Upload a part in multipart upload
    /// </summary>
    [HttpPost("multipart/upload-part")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB per part
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
    [ProducesResponseType(typeof(UploadPartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPart(
        IFormFile file,
        [FromForm] string uploadId,
        [FromForm] int partNumber,
        [FromForm] string fileName)
    {
        if (file == null || file.Length == 0) return BadRequestResponse("File is required", "FILE_REQUIRED");

        if (partNumber < 1) return BadRequestResponse("Part number must be greater than 0", "INVALID_PART_NUMBER");

        // Detect file type from fileName
        var fileType = _fileValidator.DetectFileType(fileName);
        if (!fileType.HasValue) return BadRequestResponse("Unable to determine file type from file name", "FILE_TYPE_DETECTION_FAILED");

        try
        {
            await using var stream = file.OpenReadStream();

            var eTag = await _storageService.UploadPartAsync(
                uploadId,
                fileName,
                fileType.Value,
                partNumber,
                stream);

            return OkResponse(new UploadPartResponse
            {
                PartNumber = partNumber,
                ETag = eTag
            }, "Part uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading part {PartNumber} for uploadId {UploadId}",
                partNumber, uploadId);
            return InternalErrorResponse("An error occurred while uploading the part", "PART_UPLOAD_ERROR");
        }
    }

    /// <summary>
    /// Complete multipart upload
    /// </summary>
    [HttpPost("multipart/complete")]
    [ProducesResponseType(typeof(UploadFileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteMultipartUpload(
        [FromBody] CompleteMultipartUploadRequest request)
    {
        if (request.Parts == null || request.Parts.Count == 0) return BadRequestResponse("Parts are required", "PARTS_REQUIRED");

        // Detect file type from fileName
        var fileType = _fileValidator.DetectFileType(request.FileName);
        if (!fileType.HasValue) return BadRequestResponse("Unable to determine file type from file name", "FILE_TYPE_DETECTION_FAILED");

        try
        {
            var filePath = await _storageService.CompleteMultipartUploadAsync(
                request.UploadId,
                request.FileName,
                fileType.Value,
                request.Parts);

            // Generate presigned URL with 1 hour expiry
            var url = await _storageService.GetPresignedUrlAsync(filePath, 3600);
            var expiresAt = DateTime.UtcNow.AddSeconds(3600);

            var fileInfo = await _storageService.GetFileInfoAsync(filePath);

            return OkResponse(new UploadFileResponse
            {
                FilePath = filePath,
                Url = url,
                ExpiresAt = expiresAt,
                FileSize = fileInfo.Size
            }, "Upload completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing multipart upload for uploadId {UploadId}",
                request.UploadId);
            return InternalErrorResponse("An error occurred while completing the upload", "MULTIPART_COMPLETE_ERROR");
        }
    }

    /// <summary>
    /// Abort multipart upload
    /// </summary>
    [HttpPost("multipart/abort")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AbortMultipartUpload(
        [FromBody] CompleteMultipartUploadRequest request)
    {
        // Detect file type from fileName
        var fileType = _fileValidator.DetectFileType(request.FileName);
        if (!fileType.HasValue) return BadRequestResponse("Unable to determine file type from file name", "FILE_TYPE_DETECTION_FAILED");

        try
        {
            await _storageService.AbortMultipartUploadAsync(
                request.UploadId,
                request.FileName,
                fileType.Value);

            return OkResponse("Upload aborted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aborting multipart upload for uploadId {UploadId}",
                request.UploadId);
            return InternalErrorResponse("An error occurred while aborting the upload", "MULTIPART_ABORT_ERROR");
        }
    }

    /// <summary>
    /// Get presigned URL for a file (time-limited access like Facebook)
    /// </summary>
    [HttpPost("presigned-url")]
    [ProducesResponseType(typeof(GetPresignedUrlResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPresignedUrl(
        [FromBody] GetPresignedUrlRequest request)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(request.FilePath);
            if (!exists) return NotFoundResponse("File not found", "FILE_NOT_FOUND");

            var url = await _storageService.GetPresignedUrlAsync(
                request.FilePath,
                request.ExpiryInSeconds);

            var expiresAt = DateTime.UtcNow.AddSeconds(request.ExpiryInSeconds);

            return OkResponse(new GetPresignedUrlResponse
            {
                Url = url,
                ExpiresAt = expiresAt
            }, "Presigned URL generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for {FilePath}", request.FilePath);
            return InternalErrorResponse("An error occurred while generating the URL", "PRESIGNED_URL_ERROR");
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{*filePath}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string filePath)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(filePath);
            if (!exists) return NotFoundResponse("File not found", "FILE_NOT_FOUND");

            await _storageService.DeleteFileAsync(filePath);

            return OkResponse("File deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            return InternalErrorResponse("An error occurred while deleting the file", "FILE_DELETE_ERROR");
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("info/{*filePath}")]
    [ProducesResponseType(typeof(FileInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFileInfo(string filePath)
    {
        try
        {
            var exists = await _storageService.FileExistsAsync(filePath);
            if (!exists) return NotFoundResponse("File not found", "FILE_NOT_FOUND");

            var fileInfo = await _storageService.GetFileInfoAsync(filePath);

            return OkResponse(fileInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
            return InternalErrorResponse("An error occurred while getting file information", "FILE_INFO_ERROR");
        }
    }
}