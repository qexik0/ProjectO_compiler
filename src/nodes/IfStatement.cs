using System.Text;

namespace OCompiler.nodes;

public class IfStatement : AstNode
{
    public required Expression IfConditionExpression { get; set; }
    public required Body IfBody { get; set; }
    public Body? ElseBody { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(IfStatement(Condition{IfConditionExpression})(MainBranch{IfBody})");
        if (ElseBody != null)
        {
            sb.Append($"(ElseBranch{ElseBody})");
        }
        sb.Append(")");
        return sb.ToString();
    }
}