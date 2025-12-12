using FAM.Application.Querying;

namespace FAM.Application.EmailTemplates.Shared;

/// <summary>
/// Extension methods for mapping EmailTemplate DTOs to responses
/// </summary>
public static class EmailTemplateResponseMappers
{
    /// <summary>
    /// Convert EmailTemplateDto to EmailTemplateResponse
    /// </summary>
    public static EmailTemplateResponse ToEmailTemplateResponse(this EmailTemplateDto dto)
    {
        return new EmailTemplateResponse(
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
    }

    /// <summary>
    /// Convert PageResult of EmailTemplateDto to standard paged response
    /// </summary>
    public static object ToPagedResponse(this PageResult<EmailTemplateDto> result)
    {
        return new
        {
            data = result.Items.Select(t => t.ToEmailTemplateResponse()),
            pagination = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                total = result.Total,
                totalPages = result.TotalPages,
                hasPrevPage = result.HasPrevPage,
                hasNextPage = result.HasNextPage
            }
        };
    }
}
