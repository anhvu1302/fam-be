using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FAM.Infrastructure.Common.Extensions;

/// <summary>
/// Helpers to apply a snake_case naming convention to EF Core models.
/// Centralizes logic so migrations and runtime use the same naming strategy.
/// </summary>
public static class ModelBuilderExtensions
{
    public static void ApplySnakeCaseNamingConvention(this ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.IsOwned())
            {
                continue;
            }

            // table name
            string? tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            // columns
            foreach (IMutableProperty property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // keys
            foreach (IMutableKey key in entity.GetKeys())
            {
                string? keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                {
                    key.SetName(ToSnakeCase(keyName));
                }
            }

            // foreign keys
            foreach (IMutableForeignKey fk in entity.GetForeignKeys())
            {
                string? fkName = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(fkName))
                {
                    fk.SetConstraintName(ToSnakeCase(fkName));
                }
            }

            // indexes
            foreach (IMutableIndex index in entity.GetIndexes())
            {
                string? indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(ToSnakeCase(indexName));
                }
            }
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        StringBuilder sb = new();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && name[i - 1] != '_' && !char.IsUpper(name[i - 1]))
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
