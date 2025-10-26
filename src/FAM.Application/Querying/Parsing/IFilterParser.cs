using FAM.Application.Querying.Ast;

namespace FAM.Application.Querying.Parsing;

/// <summary>
/// Interface cho filter parser
/// </summary>
public interface IFilterParser
{
    FilterNode Parse(string filter);
}
