namespace FAM.Application.Querying;

/// <summary>
/// Token type cho filter parser
/// </summary>
public enum FilterTokenType
{
    EndOfInput,
    Identifier,
    String,
    Number,
    Boolean,
    Null,
    Operator,
    LeftParen,
    RightParen,
    Comma
}

/// <summary>
/// Token trong filter DSL
/// </summary>
public sealed record FilterToken(
    FilterTokenType Type,
    string Value,
    int Position)
{
    public static FilterToken EndOfInput(int position) => new(FilterTokenType.EndOfInput, string.Empty, position);
    public static FilterToken Identifier(string value, int position) => new(FilterTokenType.Identifier, value, position);
    public static FilterToken String(string value, int position) => new(FilterTokenType.String, value, position);
    public static FilterToken Number(string value, int position) => new(FilterTokenType.Number, value, position);
    public static FilterToken Boolean(bool value, int position) => new(FilterTokenType.Boolean, value.ToString().ToLower(), position);
    public static FilterToken Null(int position) => new(FilterTokenType.Null, "null", position);
    public static FilterToken Operator(string value, int position) => new(FilterTokenType.Operator, value, position);
    public static FilterToken LeftParen(int position) => new(FilterTokenType.LeftParen, "(", position);
    public static FilterToken RightParen(int position) => new(FilterTokenType.RightParen, ")", position);
    public static FilterToken Comma(int position) => new(FilterTokenType.Comma, ",", position);
}
