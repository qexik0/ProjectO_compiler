namespace OCompiler.nodes;

public class VariableDeclaration : AstNode
{
    public required Identifier VariableIdentifier { get; set; }
    public required Expression VariableExpression { get; set; }
}