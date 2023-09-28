namespace OCompiler.nodes;

public class Parameters : AstNode
{
    public List<ParameterDeclaration> ParameterDeclarations { get; } = new List<ParameterDeclaration>();
}
