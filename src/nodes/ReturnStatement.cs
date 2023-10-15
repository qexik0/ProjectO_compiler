namespace OCompiler.nodes;

public class ReturnStatement : AstNode
{
    public Expression? ReturnExpression { get; set; }

    public override string ToString()
    {
        if (ReturnExpression == null)
        {
            return "(Return)";
        }
        return $"(Return{ReturnExpression})";
    }
}