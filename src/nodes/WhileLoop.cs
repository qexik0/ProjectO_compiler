using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class WhileLoop : AstNode
{
    public required Expression WhileConditionExpression { get; set; }
    public required Body WhileBody { get; set; }

    public void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, string currentClass, in SymbolTable<OLangSymbol> symbolTable)
    {
        var type = OLangTypeRegistry.BodyExpressionType(currentClass, WhileConditionExpression, symbolTable);
        if (type != "Boolean")
        {
            throw new Exception("If expression is not boolean");
        }

        var function = builder.InsertBlock.Parent;
        var whileEntry = function.AppendBasicBlock("whileEntry");
        var whileBody = function.AppendBasicBlock("whileBody");
        var whileExit = function.AppendBasicBlock("whileExit");

        builder.BuildBr(whileEntry);

        builder.PositionAtEnd(whileEntry);

        var condition = WhileConditionExpression.CodeGen(module, builder, symbolTable);
        builder.BuildCondBr(condition, whileBody, whileExit);

        symbolTable.EnterScope();
        builder.PositionAtEnd(whileBody);
        WhileBody.CodeGen(module, builder, currentClass, symbolTable);
        builder.BuildBr(whileEntry);
        symbolTable.ExitScope();

        builder.PositionAtEnd(whileExit);
    }

    public override string ToString()
    {
        return $"(WhileLoop(Condition{WhileConditionExpression}){WhileBody})";
    }
}