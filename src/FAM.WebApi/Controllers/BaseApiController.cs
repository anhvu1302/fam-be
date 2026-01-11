using System.Security.Claims;

using FAM.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Base API controller with common functionality for all controllers
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Get the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID</returns>
    /// <exception cref="UnauthorizedAccessException">If user ID is not found in token</exception>
    protected long GetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out long userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    /// <summary>
    /// Try to get the current authenticated user's ID from JWT claims
    /// </summary>
    /// <returns>User ID if found and valid, null otherwise</returns>
    protected long? TryGetCurrentUserId()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out long userId) ? userId : null;
    }

    /// <summary>
    /// Get the client's IP address from request headers or connection
    /// </summary>
    /// <returns>Client IP address</returns>
    protected string GetClientIpAddress()
    {
        // Priority order for getting real client IP:
        // 1. CF-Connecting-IP (Cloudflare)
        // 2. X-Forwarded-For (behind proxy/load balancer)
        // 3. X-Real-IP (nginx)
        // 4. RemoteIpAddress (direct connection)

        string? cfConnectingIp = Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfConnectingIp))
        {
            return cfConnectingIp;
        }

        string? forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            string[] ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        string? realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Get the user agent string from request headers
    /// </summary>
    /// <returns>User agent string</returns>
    protected string GetUserAgent()
    {
        return Request.Headers.UserAgent.FirstOrDefault() ?? "Unknown";
    }

    #region Response Helpers

    /// <summary>
    /// Return a successful response with data
    /// </summary>
    protected OkObjectResult OkResponse<T>(T data, string? message = null)
    {
        ApiSuccessResponse<T> response = ApiSuccessResponse<T>.Ok(data, message);
        return Ok(response);
    }

    /// <summary>
    /// Return a successful response without data
    /// </summary>
    protected OkObjectResult OkResponse(string? message = null)
    {
        ApiSuccessResponse<object> response = new(true, message, null);
        return Ok(response);
    }

    /// <summary>
    /// Return a paginated successful response
    /// </summary>
    protected OkObjectResult OkPaginatedResponse<T>(IEnumerable<T> items, int page, int pageSize, long total)
    {
        ApiPagedResponse<T> response = ApiPagedResponse<T>.Ok(items.ToList(), page, pageSize, total);
        return Ok(response);
    }

    /// <summary>
    /// Return a bad request error response
    /// </summary>
    protected BadRequestObjectResult BadRequestResponse(string message, string code = "BAD_REQUEST")
    {
        ApiErrorResponse response = ApiErrorResponse.BadRequest(message, code);
        return BadRequest(response);
    }

    /// <summary>
    /// Return an unauthorized error response
    /// </summary>
    protected UnauthorizedObjectResult UnauthorizedResponse(string message = "Unauthorized",
        string code = "UNAUTHORIZED")
    {
        ApiErrorResponse response = ApiErrorResponse.Unauthorized(message, code);
        return Unauthorized(response);
    }

    /// <summary>
    /// Return a forbidden error response
    /// </summary>
    protected ObjectResult ForbiddenResponse(string message, string code = "FORBIDDEN")
    {
        ApiErrorResponse response = ApiErrorResponse.Forbidden(message, code);
        return StatusCode(403, response);
    }

    /// <summary>
    /// Return a not found error response
    /// </summary>
    protected NotFoundObjectResult NotFoundResponse(string message, string code = "NOT_FOUND")
    {
        ApiErrorResponse response = ApiErrorResponse.NotFound(message, code);
        return NotFound(response);
    }

    /// <summary>
    /// Return a validation error response
    /// </summary>
    protected BadRequestObjectResult ValidationErrorResponse(params ApiError[] errors)
    {
        ApiErrorResponse response = ApiErrorResponse.ValidationError(errors);
        return BadRequest(response);
    }

    /// <summary>
    /// Return an internal server error response
    /// </summary>
    protected ObjectResult InternalErrorResponse(string message = "An internal server error occurred",
        string code = "INTERNAL_SERVER_ERROR")
    {
        ApiErrorResponse response = ApiErrorResponse.InternalServerError(message, code);
        return StatusCode(500, response);
    }

    #endregion
}
