using System.Text.Json;

using FAM.WebApi.Contracts.Common;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Middleware to handle 401 Unauthorized responses and convert them to standard error format
/// </summary>
public class UnauthorizedResponseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnauthorizedResponseMiddleware> _logger;

    public UnauthorizedResponseMiddleware(RequestDelegate next, ILogger<UnauthorizedResponseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Store the original response body stream
        Stream originalBodyStream = context.Response.Body;

        try
        {
            // Replace response body with a memory stream
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Call the next middleware
            await _next(context);

            // Check if response is 401
            if (context.Response.StatusCode == 401)
            {
                // Read what the response would have been
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBody).ReadToEndAsync();

                _logger.LogWarning("401 Unauthorized response detected. Original body: {Body}", responseText);

                // Clear the response body and write our standard error format
                context.Response.Body = originalBodyStream;
                context.Response.ContentType = "application/json";

                var errorResponse = new ApiErrorResponse(
                    false,
                    new List<ApiError>
                    {
                        new(
                            "Unauthorized - Token is missing, invalid, or expired",
                            "UNAUTHORIZED"
                        )
                    }
                );

                var json = JsonSerializer.Serialize(errorResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
                return;
            }

            // If not 401, copy the response body to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            // Restore the original body stream
            context.Response.Body = originalBodyStream;
        }
    }
}
