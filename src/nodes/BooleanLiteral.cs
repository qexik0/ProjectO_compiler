namespace OCompiler.nodes;

public class BooleanLiteral : AstNode
{
    public required bool Value { get; set; }

    public override string ToString()
    {
        return $"(BooleanLiteral({Value}))";
    }
}