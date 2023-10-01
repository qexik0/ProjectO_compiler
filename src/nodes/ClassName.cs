namespace OCompiler.nodes;

public class ClassName : AstNode
{
    public required Identifier ClassIdentifier { get; set; }
    public ClassName? GenericClassName { get; set; }
}