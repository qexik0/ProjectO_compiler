using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class IfStatement : AstNode
{
    public required Expression IfConditionExpression { get; set; }
    public required Body IfBody { get; set; }
    public Body? ElseBody { get; set; }

    public void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, string currentClass, in SymbolTable<OLangSymbol> symbolTable)
    {
        var type = OLangTypeRegistry.BodyExpressionType(currentClass, IfConditionExpression, symbolTable);
        if (type != "Boolean")
        {
            throw new Exception("If expression is not boolean");
        }

        var condition = IfConditionExpression.CodeGen(module, builder, symbolTable);
        
        var function = builder.InsertBlock.Parent;

        var thenBlock = function.AppendBasicBlock("then");
        var elseBlock = function.AppendBasicBlock("else");
        var continueBlock = function.AppendBasicBlock("ifcont");

        builder.BuildCondBr(condition, thenBlock, elseBlock);

        symbolTable.EnterScope();
        builder.PositionAtEnd(thenBlock);
        IfBody.CodeGen(module, builder, currentClass, symbolTable);
        builder.BuildBr(continueBlock);
        symbolTable.ExitScope();

        if (ElseBody != null)
        {
            symbolTable.EnterScope();
            builder.PositionAtEnd(elseBlock);
            ElseBody.CodeGen(module, builder, currentClass, symbolTable);
            builder.BuildBr(continueBlock);
            symbolTable.ExitScope();
        }
        else
        {
            builder.PositionAtEnd(elseBlock);
            builder.BuildBr(continueBlock);
        }

        builder.PositionAtEnd(continueBlock);
    }

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