namespace OCompiler.nodes;

public class Arguments : AstNode
{
    public List<Expression> Expressions { get; } = new List<Expression>();
}