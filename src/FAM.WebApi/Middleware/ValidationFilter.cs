using FAM.WebApi.Contracts.Common;

using FluentValidation;
using FluentValidation.Results;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Intercepts validation errors from FluentValidation and returns them in our standard format
/// { success: false, errors: [{message, code}] }
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Run FluentValidation manually for each action parameter
        foreach (ParameterDescriptor parameter in context.ActionDescriptor.Parameters)
        {
            if (context.ActionArguments.TryGetValue(parameter.Name, out object? argumentValue) && argumentValue != null)
            {
                Type argumentType = argumentValue.GetType();

                // Try to get a validator for this type
                Type validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                IValidator? validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    // Create validation context
                    ValidationContext<object> validationContext = new(argumentValue);

                    // Validate
                    ValidationResult? validationResult = await validator.ValidateAsync(validationContext);

                    if (!validationResult.IsValid)
                        // Add errors to ModelState
                    {
                        foreach (ValidationFailure? error in validationResult.Errors)
                        {
                            context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }
                }
            }
        }

        // Now check ModelState
        if (!context.ModelState.IsValid)
        {
            List<ApiError> errors = new();

            foreach ((string key, ModelStateEntry value) in context.ModelState)
            {
                if (value.Errors.Count > 0)
                {
                    foreach (ModelError error in value.Errors)
                    {
                        // Extract error code from error message if it starts with [CODE]
                        // or use a generic validation error code
                        string errorMessage = error.ErrorMessage;
                        string errorCode = "VALIDATION_ERROR";

                        // If the error message is in format "[CODE] Message", extract the code
                        if (errorMessage.StartsWith('['))
                        {
                            int endIndex = errorMessage.IndexOf(']');
                            if (endIndex > 0)
                            {
                                errorCode = errorMessage[1..endIndex];
                                errorMessage = errorMessage[(endIndex + 1)..].Trim();
                            }
                        }

                        // Add field information to error message for better context
                        string fullMessage = string.IsNullOrEmpty(key)
                            ? errorMessage
                            : $"{key}: {errorMessage}";

                        errors.Add(new ApiError(fullMessage, errorCode));
                    }
                }
            }

            ApiErrorResponse response = new(false, errors);

            context.Result = new BadRequestObjectResult(response);
            return;
        }

        await next();
    }
}
