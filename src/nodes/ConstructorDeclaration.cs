using System.Text;

namespace OCompiler.nodes;

public class ConstructorDeclaration : AstNode
{
    public Parameters? ConstructorParameters { get; set; }
    public required Body ConstructorBody { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Constructor");
        if (ConstructorParameters != null)
        {
            sb.Append(ConstructorParameters);
        }
        sb.Append($"{ConstructorBody})");
        return sb.ToString();
    }
}