using System.Text;

namespace OCompiler.nodes;

public class ClassName : AstNode
{
    public required Identifier ClassIdentifier { get; set; }
    public ClassName? GenericClassName { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(ClassName{ClassIdentifier.ToString()}");
        if (GenericClassName != null)
        {
            sb.Append($"(Generic{GenericClassName.ToString()})");
        }
        sb.Append(")");
        return sb.ToString();
    }
}