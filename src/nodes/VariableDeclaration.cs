using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class VariableDeclaration : AstNode
{
    public required Identifier VariableIdentifier { get; set; }
    public required Expression VariableExpression { get; set; }

    public void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, string curClass, SymbolTable<OLangSymbol> symbolTable)
    {
        var type = OLangTypeRegistry.BodyExpressionType(curClass, VariableExpression, symbolTable);
        symbolTable.DefineSymbol(VariableIdentifier.Name, new () {Class = type, TypeRef = OLangTypeRegistry.GetClass(type).ClassType, ValueRef = VariableExpression.CodeGen(module, builder, symbolTable)});
    }

    public override string ToString()
    {
        return $"(VariableDeclaration{VariableIdentifier}{VariableExpression})";
    }
}