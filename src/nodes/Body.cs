using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Body : AstNode
{
    public List<AstNode> StatementsOrDeclarations { get; } = new List<AstNode>();

    public unsafe void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, SymbolTable<OLangSymbol> symbolTable)
    {
        bool lastWasReturn = false;
        foreach (var line in StatementsOrDeclarations)
        {
            lastWasReturn = false;
            if (line is Statement stmnt && stmnt.StatementNode is Expression expr)
            {
                expr.CodeGen(module, builder, symbolTable);
            }
            else if (line is Statement statement && statement.StatementNode is ReturnStatement returnStatement)
            {
                if (returnStatement.ReturnExpression != null)
                {
                    builder.BuildRet(returnStatement.ReturnExpression.CodeGen(module, builder, symbolTable));
                }
                else
                {
                    builder.BuildRetVoid();
                }
                lastWasReturn = true;
            }
        }
        if (!lastWasReturn)
        {
            builder.BuildRetVoid();
        }
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