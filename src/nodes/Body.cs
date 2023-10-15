using System.Text;

namespace OCompiler.nodes;

public class Body : AstNode
{
    public List<AstNode> StatementsOrDeclarations { get; } = new List<AstNode>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(BodyConstruct");
        foreach (var stOrDecl in StatementsOrDeclarations)
        {
            sb.Append(stOrDecl);
        }
        sb.Append(")");
        return sb.ToString();
    }
}