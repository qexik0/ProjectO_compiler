using System.Text;

namespace OCompiler.nodes;

public class ConstructorCall : AstNode
{
    public required ClassName ConstructorClassName { get; set; }
    public Arguments? ConstructorArguments { get; set; }

    public void CodeGen()
    {
        
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(ConstructorCall{ConstructorClassName}");
        if (ConstructorArguments != null)
        {
            sb.Append(ConstructorArguments);
        }
        sb.Append(")");
        return sb.ToString();
    }
}