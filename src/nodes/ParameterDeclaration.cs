namespace OCompiler.nodes;

public class ParameterDeclaration : AstNode
{
    public required Identifier ParameterIdentifier { get; set; }
    public required ClassName ParameterClassName { get; set; }
}