using FAM.Application.EmailTemplates.Commands.ActivateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.CreateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.DeactivateEmailTemplate;
using FAM.Application.EmailTemplates.Commands.DeleteEmailTemplate;
using FAM.Application.EmailTemplates.Commands.UpdateEmailTemplate;
using FAM.Application.EmailTemplates.Queries;
using FAM.Application.EmailTemplates.Queries.GetAllEmailTemplates;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplateByCode;
using FAM.Application.EmailTemplates.Queries.GetEmailTemplateById;
using FAM.Application.EmailTemplates.Shared;
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
    /// Get all email templates
    /// </summary>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="category">Filter by category (1=Authentication, 2=Notification, 3=Marketing, 4=System)</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmailTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmailTemplateResponse>>> GetAllTemplates(
        [FromQuery] bool? isActive = null,
        [FromQuery] int? category = null)
    {
        var query = new GetAllEmailTemplatesQuery
        {
            IsActive = isActive,
            Category = category
        };

        var result = await _mediator.Send(query);
        
        var response = result.Select(dto => new EmailTemplateResponse(
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
        )).ToList();
        
        return Ok(response);
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(EmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateResponse>> GetTemplateById(long id)
    {
        var query = new GetEmailTemplateByIdQuery(id);
        var dto = await _mediator.Send(query);

        if (dto == null)
            return NotFound();

        var response = new EmailTemplateResponse(
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

        return Ok(response);
    }

    /// <summary>
    /// Get email template by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(EmailTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateResponse>> GetTemplateByCode(string code)
    {
        var query = new GetEmailTemplateByCodeQuery(code);
        var dto = await _mediator.Send(query);

        if (dto == null)
            return NotFound();

        var response = new EmailTemplateResponse(
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

        return Ok(response);
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(long), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<long>> CreateTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new CreateEmailTemplateCommand
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

        var templateId = await _mediator.Send(command);

        _logger.LogInformation("Email template created: {TemplateId} - {TemplateCode}", templateId, request.Code);

        return CreatedAtAction(nameof(GetTemplateById), new { id = templateId }, templateId);
    }

    /// <summary>
    /// Update an existing email template
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateTemplate(long id, [FromBody] UpdateEmailTemplateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var command = new UpdateEmailTemplateCommand
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

        _logger.LogInformation("Email template updated: {TemplateId}", id);

        return Ok();
    }

    /// <summary>
    /// Delete an email template (soft delete)
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteTemplate(long id)
    {
        var command = new DeleteEmailTemplateCommand(id);
        await _mediator.Send(command);

        _logger.LogInformation("Email template deleted: {TemplateId}", id);

        return NoContent();
    }

    /// <summary>
    /// Activate an email template
    /// </summary>
    [HttpPost("{id:long}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateTemplate(long id)
    {
        var command = new ActivateEmailTemplateCommand(id);
        await _mediator.Send(command);

        _logger.LogInformation("Email template activated: {TemplateId}", id);

        return Ok();
    }

    /// <summary>
    /// Deactivate an email template
    /// </summary>
    [HttpPost("{id:long}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateTemplate(long id)
    {
        var command = new DeactivateEmailTemplateCommand(id);
        await _mediator.Send(command);

        _logger.LogInformation("Email template deactivated: {TemplateId}", id);

        return Ok();
    }
}
