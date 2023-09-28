namespace OCompiler.nodes;

public class ClassName : AstNode
{
    public required Identifier ClassIdentifier { get; set; }
}