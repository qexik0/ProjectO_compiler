using System.Text;
using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class Body : AstNode
{
    public List<AstNode> StatementsOrDeclarations { get; } = new List<AstNode>();

    public unsafe void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder)
    {
        foreach (var line in StatementsOrDeclarations)
        {
            if (line is Statement stmnt && stmnt.StatementNode is Expression expr)
            {
                expr.CodeGen(module, builder, new());
            }
        }
        builder.BuildRetVoid();
    }

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