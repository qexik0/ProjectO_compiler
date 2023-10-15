using System.Text;

namespace OCompiler.nodes;

public class Arguments : AstNode
{
    public List<Expression> Expressions { get; } = new List<Expression>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Arguments");
        foreach (var expr in Expressions)
        {
            sb.Append(expr);
        }
        sb.Append(")");
        return sb.ToString();
    }
}