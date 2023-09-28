namespace OCompiler.nodes;

public class WhileLoop : AstNode
{
    public required Expression WhileConditionExpression { get; set; }
    public required Body WileBody { get; set; }
}