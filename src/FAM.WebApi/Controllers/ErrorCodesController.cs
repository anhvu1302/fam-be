using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// API endpoints for error codes management.
/// Provides error codes for frontend internationalization (i18n).
/// Only available in Development environment.
/// </summary>
[ApiController]
[Route("/error-codes")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger in Production
public class ErrorCodesController : BaseApiController
{
    private readonly IWebHostEnvironment _environment;

    public ErrorCodesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet]
    public IActionResult GetAllErrorCodes()
    {
        // Only allow in Development environment
        if (!_environment.IsDevelopment()) return Forbid("This API is only available in Development environment");

        IReadOnlyDictionary<string, string> errorCodesDict = ErrorMessages.GetAllErrorCodes();

        var errorCodesList = errorCodesDict
            .Select(kvp => new ErrorCodeResponse
            {
                Code = kvp.Key,
                Message = kvp.Value
            })
            .OrderBy(x => x.Code)
            .ToList();

        var response = new ErrorCodesListResponse
        {
            ErrorCodes = errorCodesList,
            TotalCount = errorCodesList.Count
        };

        return OkResponse(response);
    }
}
