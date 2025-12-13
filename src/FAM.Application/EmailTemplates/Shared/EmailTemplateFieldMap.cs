using System.Linq.Expressions;

using FAM.Application.Querying;
using FAM.Application.Querying.Validation;
using FAM.Domain.EmailTemplates;

namespace FAM.Application.EmailTemplates.Shared;

/// <summary>
/// Field map for EmailTemplate domain entity - defines allowed fields for filtering and sorting
/// </summary>
public class EmailTemplateFieldMap : BaseFieldMap<EmailTemplate>
{
    private static readonly Lazy<EmailTemplateFieldMap> _instance = new(() => new EmailTemplateFieldMap());

    private EmailTemplateFieldMap()
    {
    }

    public static EmailTemplateFieldMap Instance => _instance.Value;

    public override FieldMap<EmailTemplate> Fields { get; } = new FieldMap<EmailTemplate>()
        .Add("id", t => t.Id)
        .Add("code", t => t.Code)
        .Add("name", t => t.Name)
        .Add("subject", t => t.Subject)
        .Add("htmlBody", t => t.HtmlBody)
        .Add("plainTextBody", t => t.PlainTextBody!)
        .Add("description", t => t.Description!)
        .Add("category", t => t.Category)
        .Add("isActive", t => t.IsActive)
        .Add("isSystem", t => t.IsSystem)
        .Add("createdAt", t => t.CreatedAt)
        .Add("createdById", t => t.CreatedById)
        .Add("updatedAt", t => t.UpdatedAt!)
        .Add("updatedById", t => t.UpdatedById)
        .Add("isDeleted", t => t.IsDeleted)
        .Add("deletedAt", t => t.DeletedAt!);

    protected override Dictionary<string, Expression<Func<EmailTemplate, object>>> AllowedIncludes { get; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // EmailTemplate doesn't have navigation properties to include
        };
}
