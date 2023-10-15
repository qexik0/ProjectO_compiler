namespace OCompiler.nodes;

public class Primary : AstNode
{
    public required AstNode Node { get; set; }

    public override string ToString()
    {
        return $"(Primary{Node})";
    }
}