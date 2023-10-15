using System.Text;

namespace OCompiler.nodes;

public class ClassDeclaration : AstNode
{
    public required ClassName Name { get; set; }
    public ClassName? BaseClassName { get; set; }
    public List<MemberDeclaration> Members { get; } = new List<MemberDeclaration>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(Class{Name.ToString()}");
        if (BaseClassName != null)
        {
            sb.Append($"(extends({BaseClassName.ToString()}))");
        }
        foreach (var member in Members)
        {
            sb.Append($"{member.ToString()}");
        }
        sb.Append(")");
        return sb.ToString();
    }
}