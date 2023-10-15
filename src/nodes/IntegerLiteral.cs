namespace OCompiler.nodes;

public class IntegerLiteral : AstNode
{
    public required int Value { get; set; }

    public override string ToString()
    {
        return $"(IntegerLiteral({Value}))";
    }
}