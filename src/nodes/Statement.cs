namespace OCompiler.nodes;

public class Statement : AstNode
{
    public required AstNode StatementNode { get; set; }

    public override string ToString()
    {
        return $"(Statement{StatementNode})";
    }
}