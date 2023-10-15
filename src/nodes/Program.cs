using System.Text;

namespace OCompiler.nodes;

public class Program : AstNode
{
    public List<ClassDeclaration> ProgramClasses { get; } = new List<ClassDeclaration>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Program");
        foreach (var classDecl in ProgramClasses)
        {
            sb.Append(classDecl.ToString());
        }
        sb.Append(")");
        return sb.ToString();
    }
}