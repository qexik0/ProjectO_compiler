namespace OCompiler.nodes;

public class Body : AstNode
{
    public List<AstNode> StatementsOrDeclarations { get; } = new List<AstNode>();
}