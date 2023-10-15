namespace OCompiler.nodes;

public class RealLiteral : AstNode
{
    public required double Value { get; set; }

    public override string ToString()
    {
        return $"(RealLiteral({Value}))";
    }
}