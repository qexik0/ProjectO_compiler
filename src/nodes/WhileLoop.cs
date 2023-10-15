namespace OCompiler.nodes;

public class WhileLoop : AstNode
{
    public required Expression WhileConditionExpression { get; set; }
    public required Body WhileBody { get; set; }

    public override string ToString()
    {
        return $"(WhileLoop(Condition{WhileConditionExpression}){WhileBody})";
    }
}