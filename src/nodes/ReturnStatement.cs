namespace OCompiler.nodes;

public class ReturnStatement : AstNode
{
    public Expression? ReturnExpression { get; set; }
}