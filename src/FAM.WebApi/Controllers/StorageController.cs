using FAM.Application.Abstractions;
using FAM.Application.Storage.Commands;
using FAM.Application.Storage.Shared;
using FAM.WebApi.Contracts.Common;
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
    /// <remarks>
    /// Initializes an upload session and returns details for uploading a file to cloud storage.
    /// This step is required before uploading any file.
    /// 
    /// Request body:
    /// {
    ///   "fileName": "document.pdf",
    ///   "contentType": "application/pdf",
    ///   "fileSize": 1048576,
    ///   "idempotencyKey": "unique-key-12345"
    /// }
    /// 
    /// Example: POST /api/storage/sessions/init
    /// </remarks>
    /// <param name="request">InitUploadSessionRequest with file details</param>
    /// <response code="200">Success - Returns {success: true, result: InitUploadSessionResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid file data", code: "INVALID_UPLOAD_SESSION"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "USER_NOT_AUTHENTICATED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while initializing upload session", code: "UPLOAD_SESSION_ERROR"}]}</response>
    [HttpPost("sessions/init")]
    [ProducesResponseType(typeof(ApiSuccessResponse<InitUploadSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
    /// <remarks>
    /// Uploads a single file to cloud storage. Maximum file size is 100 MB.
    /// Automatic file type detection is performed.
    /// 
    /// Supported file types: PDF, Images (JPG, PNG, GIF), Documents (DOC, DOCX, XLS, XLSX)
    /// 
    /// Request: Multipart form data with "file" parameter
    /// Example: POST /api/storage/upload
    /// 
    /// Returns presigned URL with 1 hour expiry.
    /// </remarks>
    /// <param name="file">File to upload (max 100 MB)</param>
    /// <response code="200">Success - Returns {success: true, message: "File uploaded successfully", result: UploadFileResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "File is required", code: "FILE_REQUIRED"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while uploading the file", code: "FILE_UPLOAD_ERROR"}]}</response>
    [HttpPost("upload")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB max
    [RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UploadFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequestResponse("File is required", "FILE_REQUIRED");

        // Validate file and auto-detect type
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            file.FileName,
            file.Length);

        if (!isValid || !fileType.HasValue) return BadRequestResponse(errorMessage ?? "File validation failed", "FILE_VALIDATION_FAILED");

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

            return OkResponse(new UploadFileResponse(filePath, url, expiresAt, file.Length), "File uploaded successfully");
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
    /// <remarks>
    /// Initiates a multipart upload session for large files (typically > 100 MB).
    /// Returns upload ID and recommended chunk size (5 MB).
    /// 
    /// Request body:
    /// {
    ///   "fileName": "largefile.zip",
    ///   "contentType": "application/zip",
    ///   "totalSize": 524288000
    /// }
    /// 
    /// Process:
    /// 1. Initiate multipart upload (this endpoint)
    /// 2. Upload each part with uploadId and part number
    /// 3. Complete upload with all ETags
    /// 
    /// Example: POST /api/storage/multipart/initiate
    /// </remarks>
    /// <param name="request">InitiateMultipartUploadRequest with file details</param>
    /// <response code="200">Success - Returns {success: true, message: "Multipart upload initiated", result: InitiateMultipartUploadResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "File validation failed", code: "FILE_VALIDATION_FAILED"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while initiating multipart upload", code: "MULTIPART_INIT_ERROR"}]}</response>
    [HttpPost("multipart/initiate")]
    [ProducesResponseType(typeof(ApiSuccessResponse<InitiateMultipartUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitiateMultipartUpload(
        [FromBody] InitiateMultipartUploadRequest request)
    {
        // Validate file and auto-detect type
        var (isValid, errorMessage, fileType) = _fileValidator.ValidateFile(
            request.FileName,
            request.TotalSize);

        if (!isValid || !fileType.HasValue) return BadRequestResponse(errorMessage ?? "File validation failed", "FILE_VALIDATION_FAILED");

        try
        {
            var uploadId = await _storageService.InitiateMultipartUploadAsync(
                request.FileName,
                fileType.Value,
                request.ContentType);

            return OkResponse(new InitiateMultipartUploadResponse(
                uploadId,
                uploadId.Split('_')[0], // Extract object name
                5 * 1024 * 1024 // 5 MB recommended chunk size
            ), "Multipart upload initiated");
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
    /// <remarks>
    /// Uploads a single part of a multipart upload. Maximum 50 MB per part.
    /// 
    /// Request: Multipart form data with parameters:
    /// - file: The file part (max 50 MB)
    /// - uploadId: Upload session ID from initiate endpoint
    /// - partNumber: Part sequence number (1-based)
    /// - fileName: Original file name
    /// 
    /// Response includes ETag for part tracking.
    /// 
    /// Example: POST /api/storage/multipart/upload-part
    /// </remarks>
    /// <param name="file">File part to upload (max 50 MB)</param>
    /// <param name="uploadId">Upload session ID</param>
    /// <param name="partNumber">Part number (must be > 0)</param>
    /// <param name="fileName">Original file name</param>
    /// <response code="200">Success - Returns {success: true, message: "Part uploaded successfully", result: UploadPartResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "File is required", code: "FILE_REQUIRED"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while uploading the part", code: "PART_UPLOAD_ERROR"}]}</response>
    [HttpPost("multipart/upload-part")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB per part
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiSuccessResponse<UploadPartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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

            return OkResponse(new UploadPartResponse(partNumber, eTag), "Part uploaded successfully");
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
    /// <remarks>
    /// Completes a multipart upload session. All parts must be uploaded before calling this.
    /// 
    /// Request body:
    /// {
    ///   "uploadId": "upload-session-id",
    ///   "fileName": "largefile.zip",
    ///   "parts": [
    ///     {"partNumber": 1, "eTag": "etag-1"},
    ///     {"partNumber": 2, "eTag": "etag-2"}
    ///   ]
    /// }
    /// 
    /// Returns presigned URL with 1 hour expiry.
    /// 
    /// Example: POST /api/storage/multipart/complete
    /// </remarks>
    /// <param name="request">CompleteMultipartUploadRequest with upload details</param>
    /// <response code="200">Success - Returns {success: true, message: "Upload completed successfully", result: UploadFileResponse}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Parts are required", code: "PARTS_REQUIRED"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while completing the upload", code: "MULTIPART_COMPLETE_ERROR"}]}</response>
    [HttpPost("multipart/complete")]
    [ProducesResponseType(typeof(ApiSuccessResponse<UploadFileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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

            return OkResponse(new UploadFileResponse(filePath, url, expiresAt, fileInfo.Size), "Upload completed successfully");
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
    /// <remarks>
    /// Cancels an ongoing multipart upload session. All uploaded parts are deleted.
    /// Can be used to clean up failed upload attempts.
    /// 
    /// Example: POST /api/storage/multipart/abort
    /// </remarks>
    /// <param name="request">CompleteMultipartUploadRequest with uploadId and fileName</param>
    /// <response code="200">Success - Returns {success: true, message: "Upload aborted successfully"}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Unable to determine file type from file name", code: "FILE_TYPE_DETECTION_FAILED"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while aborting the upload", code: "MULTIPART_ABORT_ERROR"}]}</response>
    [HttpPost("multipart/abort")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
    /// Get presigned URL for a file (time-limited access)
    /// </summary>
    /// <remarks>
    /// Generates a temporary presigned URL for accessing a file without authentication.
    /// Similar to Facebook's time-limited file sharing.
    /// 
    /// Request body:
    /// {
    ///   "filePath": "assets/document.pdf",
    ///   "expiryInSeconds": 3600
    /// }
    /// 
    /// URL expires after specified seconds. Default: 1 hour (3600 seconds)
    /// 
    /// Example: POST /api/storage/presigned-url
    /// </remarks>
    /// <param name="request">GetPresignedUrlRequest with filePath and expiryInSeconds</param>
    /// <response code="200">Success - Returns {success: true, message: "Presigned URL generated successfully", result: GetPresignedUrlResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "File not found", code: "FILE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while generating the URL", code: "PRESIGNED_URL_ERROR"}]}</response>
    [HttpPost("presigned-url")]
    [ProducesResponseType(typeof(ApiSuccessResponse<GetPresignedUrlResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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

            return OkResponse(new GetPresignedUrlResponse(url, expiresAt), "Presigned URL generated successfully");
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
    /// <remarks>
    /// Permanently deletes a file from cloud storage. Cannot be undone.
    /// 
    /// Example: DELETE /api/storage/assets/document.pdf
    /// </remarks>
    /// <param name="filePath">File path to delete</param>
    /// <response code="204">Success - No content returned</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "File not found", code: "FILE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while deleting the file", code: "FILE_DELETE_ERROR"}]}</response>
    [HttpDelete("{*filePath}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
    /// <remarks>
    /// Retrieves metadata information about a file including size, creation date, and type.
    /// 
    /// Example: GET /api/storage/info/assets/document.pdf
    /// </remarks>
    /// <param name="filePath">File path to get information for</param>
    /// <response code="200">Success - Returns {success: true, result: FileInfo}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "File not found", code: "FILE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "An error occurred while getting file information", code: "FILE_INFO_ERROR"}]}</response>
    [HttpGet("info/{*filePath}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<FileInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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