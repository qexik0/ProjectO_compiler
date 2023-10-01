namespace OCompiler.nodes;

public class IfStatement : AstNode
{
    public required Expression IfConditionExpression { get; set; }
    public required Body IfBody { get; set; }
    public Body? ElseBody { get; set; }
}