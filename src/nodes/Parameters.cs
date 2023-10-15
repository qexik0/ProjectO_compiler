using System.Text;

namespace OCompiler.nodes;

public class Parameters : AstNode
{
    public List<ParameterDeclaration> ParameterDeclarations { get; } = new List<ParameterDeclaration>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Parameters");
        foreach (var decl in ParameterDeclarations)
        {
            sb.Append(decl);
        }
        sb.Append(")");
        return sb.ToString();
    }
}
