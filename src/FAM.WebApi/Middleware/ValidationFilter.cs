using FAM.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Intercepts validation errors from FluentValidation and returns them in our standard format
/// { success: false, errors: [{message, code}] }
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = new List<ApiError>();

            foreach ((var key, ModelStateEntry value) in context.ModelState)
                if (value.Errors.Count > 0)
                    foreach (ModelError error in value.Errors)
                    {
                        // Extract error code from error message if it starts with [CODE]
                        // or use a generic validation error code
                        var errorMessage = error.ErrorMessage;
                        var errorCode = "VALIDATION_ERROR";

                        // If the error message is in format "[CODE] Message", extract the code
                        if (errorMessage.StartsWith('['))
                        {
                            var endIndex = errorMessage.IndexOf(']');
                            if (endIndex > 0)
                            {
                                errorCode = errorMessage[1..endIndex];
                                errorMessage = errorMessage[(endIndex + 1)..].Trim();
                            }
                        }

                        // Add field information to error message for better context
                        var fullMessage = string.IsNullOrEmpty(key)
                            ? errorMessage
                            : $"{key}: {errorMessage}";

                        errors.Add(new ApiError(fullMessage, errorCode));
                    }

            var response = new ApiErrorResponse(false, errors);

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
