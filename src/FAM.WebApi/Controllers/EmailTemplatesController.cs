using FAM.Application.EmailTemplates.Commands.ActivateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.CreateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.DeactivateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.DeleteEmailTemplate;
using FAM.Application.EmailTemplates.Commands.UpdateEmailTemplate;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplateByCode;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplateById;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplates;
using FAM.Application.EmailTemplates.Shared;
using FAM.Application.Querying;
using FAM.Application.Querying.Extensions;
using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Common;
using FAM.WebApi.Contracts.EmailTemplates;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Email template management controller
/// Manages transactional email templates
/// </summary>
[ApiController]
[Route("api/email-templates")]
[Authorize]
public class EmailTemplatesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmailTemplatesController> _logger;

    public EmailTemplatesController(IMediator mediator, ILogger<EmailTemplatesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of email templates with advanced filtering and sorting
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of email templates. Supports filtering, sorting, field selection, and includes.
    /// 
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 10)
    /// - sortBy: Field to sort by (e.g., "id", "code", "name", "category")
    /// - sortOrder: Sort order - "asc" or "desc" (default: asc)
    /// - fields: Comma-separated fields to include in response
    /// - filter: Filter expression (e.g., "isActive=true", "category=1")
    /// 
    /// Example: GET /api/email-templates?page=1&amp;pageSize=10&amp;sortBy=name&amp;sortOrder=asc&amp;filter=isActive=true
    /// </remarks>
    /// <param name="parameters">Query parameters for pagination, filtering, sorting, field selection and includes</param>
    /// <response code="200">Success - Returns {success: true, result: {items: [EmailTemplate], totalCount, pageNumber, pageSize, totalPages}}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid query parameters", code: "INVALID_PARAMETERS"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiSuccessResponse<PageResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmailTemplates([FromQuery] PaginationQueryParameters parameters)
    {
        GetEmailTemplatesQuery query = new(parameters.ToQueryRequest());
        PageResult<EmailTemplateDto> result = await _mediator.Send(query);

        // Apply field selection if requested
        string[]? fields = parameters.GetFieldsArray();
        if (fields != null && fields.Length > 0)
        {
            object selectedResult = result.SelectFieldsToResponse(fields);
            return OkResponse(selectedResult);
        }

        return OkResponse(result.ToPagedResponse());
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    /// <remarks>
    /// Returns a specific email template by ID with all details including available placeholders.
    /// 
    /// Example: GET /api/email-templates/1
    /// </remarks>
    /// <param name="id">Email template ID</param>
    /// <response code="200">Success - Returns {success: true, result: EmailTemplateResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with ID {id} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<EmailTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmailTemplateResponse>> GetTemplateById(long id)
    {
        GetEmailTemplateByIdQuery query = new(id);
        EmailTemplateDto? dto = await _mediator.Send(query);

        if (dto == null)
        {
            throw new NotFoundException(ErrorCodes.GEN_NOT_FOUND, "EmailTemplate", id);
        }

        EmailTemplateResponse response = new(
            dto.Id,
            dto.Code,
            dto.Name,
            dto.Subject,
            dto.HtmlBody,
            dto.PlainTextBody,
            dto.Description,
            dto.AvailablePlaceholders,
            dto.IsActive,
            dto.IsSystem,
            dto.Category,
            dto.CategoryName,
            dto.CreatedAt,
            dto.UpdatedAt
        );

        return OkResponse(response);
    }

    /// <summary>
    /// Get email template by code
    /// </summary>
    /// <remarks>
    /// Returns a specific email template by unique code identifier.
    /// 
    /// Example: GET /api/email-templates/by-code/VERIFY_EMAIL
    /// </remarks>
    /// <param name="code">Email template code (unique identifier)</param>
    /// <response code="200">Success - Returns {success: true, result: EmailTemplateResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with code {code} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<EmailTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EmailTemplateResponse>> GetTemplateByCode(string code)
    {
        GetEmailTemplateByCodeQuery query = new(code);
        EmailTemplateDto? dto = await _mediator.Send(query);

        if (dto == null)
        {
            throw new NotFoundException(ErrorCodes.GEN_NOT_FOUND, "EmailTemplate", code);
        }

        EmailTemplateResponse response = new(
            dto.Id,
            dto.Code,
            dto.Name,
            dto.Subject,
            dto.HtmlBody,
            dto.PlainTextBody,
            dto.Description,
            dto.AvailablePlaceholders,
            dto.IsActive,
            dto.IsSystem,
            dto.Category,
            dto.CategoryName,
            dto.CreatedAt,
            dto.UpdatedAt
        );

        return OkResponse(response);
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    /// <remarks>
    /// Creates a new email template with the specified properties.
    /// 
    /// Request body:
    /// {
    ///   "code": "VERIFY_EMAIL",
    ///   "name": "Email Verification",
    ///   "subject": "Please verify your email address",
    ///   "htmlBody": "&lt;html&gt;...&lt;/html&gt;",
    ///   "plainTextBody": "Verify your email...",
    ///   "description": "Sent to users to verify their email address",
    ///   "availablePlaceholders": ["{{VerificationLink}}", "{{UserName}}"],
    ///   "category": 1
    /// }
    /// 
    /// Only system administrators can create templates. System templates cannot be created via API.
    /// 
    /// Example: POST /api/email-templates
    /// </remarks>
    /// <param name="request">CreateEmailTemplateRequest with template details</param>
    /// <response code="201">Created - Returns {success: true, result: long (templateId)}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid template data", code: "INVALID_TEMPLATE_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="409">Conflict - Returns {success: false, errors: [{message: "Template with code {code} already exists", code: "TEMPLATE_ALREADY_EXISTS"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiSuccessResponse<long>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<long>> CreateTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        CreateEmailTemplateCommand command = new()
        {
            Code = request.Code,
            Name = request.Name,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            PlainTextBody = request.PlainTextBody,
            Description = request.Description,
            AvailablePlaceholders = request.AvailablePlaceholders,
            Category = request.Category,
            IsSystem = false // Only seeder can create system templates
        };

        long templateId = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetTemplateById), new { id = templateId }, templateId);
    }

    /// <summary>
    /// Update an existing email template
    /// </summary>
    /// <remarks>
    /// Updates an existing email template with new values.
    /// 
    /// Request body:
    /// {
    ///   "name": "Updated Template Name",
    ///   "subject": "Updated subject",
    ///   "htmlBody": "&lt;html&gt;...&lt;/html&gt;",
    ///   "plainTextBody": "Updated text...",
    ///   "description": "Updated description",
    ///   "availablePlaceholders": ["{{Placeholder1}}", "{{Placeholder2}}"],
    ///   "category": 1
    /// }
    /// 
    /// Only editable fields in the template can be updated. Code cannot be changed.
    /// System templates can only be updated by system administrators.
    /// 
    /// Example: PUT /api/email-templates/1
    /// </remarks>
    /// <param name="id">Email template ID to update</param>
    /// <param name="request">UpdateEmailTemplateRequest with template details to update</param>
    /// <response code="200">Success - Returns {success: true, result: EmailTemplateResponse, message: "Email template updated successfully"}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Invalid template data", code: "INVALID_TEMPLATE_DATA"}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with ID {id} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateTemplate(long id, [FromBody] UpdateEmailTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        UpdateEmailTemplateCommand command = new()
        {
            Id = id,
            Name = request.Name,
            Subject = request.Subject,
            HtmlBody = request.HtmlBody,
            PlainTextBody = request.PlainTextBody,
            Description = request.Description,
            AvailablePlaceholders = request.AvailablePlaceholders,
            Category = request.Category
        };

        await _mediator.Send(command);

        return OkResponse();
    }

    /// <summary>
    /// Delete an email template (soft delete)
    /// </summary>
    /// <remarks>
    /// Soft-deletes an email template by marking it as deleted.
    /// Deleted templates cannot be used but historical records are preserved.
    /// System templates cannot be deleted via API.
    /// 
    /// Example: DELETE /api/email-templates/1
    /// </remarks>
    /// <param name="id">Email template ID to delete</param>
    /// <response code="204">Success - No content returned</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with ID {id} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="400">Bad Request - Returns {success: false, errors: [{message: "Cannot delete system template", code: "SYSTEM_TEMPLATE_DELETE_FAILED"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteTemplate(long id)
    {
        DeleteEmailTemplateCommand command = new(id);
        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Activate an email template
    /// </summary>
    /// <remarks>
    /// Activates a deactivated email template, making it available for sending.
    /// Only deactivated templates can be activated.
    /// 
    /// Example: POST /api/email-templates/1/activate
    /// </remarks>
    /// <param name="id">Email template ID to activate</param>
    /// <response code="200">Success - Returns {success: true, result: EmailTemplateResponse, message: "Email template activated successfully"}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with ID {id} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPost("{id:long}/activate")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ActivateTemplate(long id)
    {
        ActivateEmailTemplateCommand command = new(id);
        await _mediator.Send(command);

        return OkResponse();
    }

    /// <summary>
    /// Deactivate an email template
    /// </summary>
    /// <remarks>
    /// Deactivates an email template, preventing it from being used for sending.
    /// Deactivated templates can be reactivated later.
    /// 
    /// Example: POST /api/email-templates/1/deactivate
    /// </remarks>
    /// <param name="id">Email template ID to deactivate</param>
    /// <response code="200">Success - Returns {success: true, result: EmailTemplateResponse, message: "Email template deactivated successfully"}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: "User not authenticated", code: "UNAUTHORIZED"}]}</response>
    /// <response code="404">Not Found - Returns {success: false, errors: [{message: "Email template with ID {id} not found", code: "TEMPLATE_NOT_FOUND"}]}</response>
    /// <response code="500">Internal Server Error - Returns {success: false, errors: [{message: "Internal server error", code: "INTERNAL_ERROR"}]}</response>
    [HttpPost("{id:long}/deactivate")]
    [ProducesResponseType(typeof(ApiSuccessResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeactivateTemplate(long id)
    {
        DeactivateEmailTemplateCommand command = new(id);
        await _mediator.Send(command);

        return OkResponse();
    }
}
