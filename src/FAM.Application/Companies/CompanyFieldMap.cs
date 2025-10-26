using FAM.Application.Querying.Validation;
using FAM.Domain.Companies;

namespace FAM.Application.Companies;

/// <summary>
/// FieldMap cho Company entity - định nghĩa các field có thể filter/sort
/// </summary>
public static class CompanyFieldMap
{
    private static FieldMap<Company>? _instance;

    public static FieldMap<Company> Instance => _instance ??= CreateFieldMap();

    private static FieldMap<Company> CreateFieldMap()
    {
        return new FieldMap<Company>()
            .Add("id", x => x.Id, canFilter: true, canSort: true)
            .Add("name", x => x.Name, canFilter: true, canSort: true)
            .Add("taxcode", x => x.TaxCode, canFilter: true, canSort: true)
            .Add("address", x => x.Address, canFilter: true, canSort: true)
            .Add("createdAt", x => x.CreatedAt, canFilter: true, canSort: true)
            .Add("updatedAt", x => x.UpdatedAt, canFilter: true, canSort: true);
    }
}
