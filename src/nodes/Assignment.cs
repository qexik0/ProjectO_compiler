using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Assignment : AstNode
{
    public required Identifier AssignmentIdentifier { get; set; }
    public required Expression AssignmentExpression { get; set; }

    public void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, string curClass, SymbolTable<OLangSymbol> symbolTable)
    {
        var symbol = symbolTable.FindSymbol(AssignmentIdentifier.Name);
        var newPtr = builder.BuildAlloca(symbol.TypeRef);
        builder.BuildStore(AssignmentExpression.CodeGen(module, builder, symbolTable, curClass), newPtr);
        symbol.ValueRef = builder.BuildLoad2(symbol.TypeRef, newPtr);
    }

    public override string ToString()
    {
        return $"(Assignment{AssignmentIdentifier}{AssignmentExpression})";
    }
}