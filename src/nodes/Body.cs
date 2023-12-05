using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Body : AstNode
{
    public List<AstNode> StatementsOrDeclarations { get; } = new List<AstNode>();

    public unsafe void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, string curClass, SymbolTable<OLangSymbol> symbolTable)
    {
        foreach (var line in StatementsOrDeclarations)
        {
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
            }
            else if (line is Statement st && st.StatementNode is IfStatement ifStatement)
            {
                ifStatement.CodeGen(module, builder, curClass, symbolTable);
            }
            else if (line is Statement stat && stat.StatementNode is WhileLoop whileLoop)
            {
                whileLoop.CodeGen(module, builder, curClass, symbolTable);
            }
            else if (line is VariableDeclaration decl)
            {
                decl.CodeGen(module, builder, curClass, symbolTable);
            }
            else if (line is Statement statemnt && statemnt.StatementNode is Assignment ass)
            {
                ass.CodeGen(module, builder, curClass, symbolTable);
            }
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