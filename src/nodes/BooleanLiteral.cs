namespace OCompiler.nodes;

public class BooleanLiteral : AstNode
{
    public required bool Value { get; set; }
}