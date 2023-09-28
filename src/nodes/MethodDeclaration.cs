namespace OCompiler.nodes;

public class MethodDeclaration : AstNode
{
    public required Identifier MethodIdentifier { get; set; }
    public Parameters? MethodParameters { get; set; }
    public Identifier? ReturnTypeIdentifier { get; set; }
    public required Body MethodBody { get; set; }
}