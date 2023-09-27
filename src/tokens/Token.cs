namespace OCompiler;

public class Token
{
    public required TokenType Type { get; init; }
    public required string Value { get; init; }
    public required int LineNumber { get; init; }
    public required int ColumnNumber { get; init; }
}