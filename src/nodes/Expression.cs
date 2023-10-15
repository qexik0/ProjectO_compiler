using System.Text;

namespace OCompiler.nodes;

public class Expression : AstNode
{
    public required AstNode PrimaryOrConstructorCall { get; set; }
    public List<(Identifier, Arguments?)> Calls { get; } = new List<(Identifier, Arguments?)>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(Expression{PrimaryOrConstructorCall}");
        foreach (var call in Calls)
        {
            sb.Append($"(Call{call.Item1}{call.Item2})");
        }
        sb.Append(")");
        return sb.ToString();
    }
}