using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class MethodDeclaration : AstNode
{
    public required Identifier MethodIdentifier { get; set; }
    public Parameters? MethodParameters { get; set; }
    public ClassName? ReturnTypeIdentifier { get; set; }
    public required Body MethodBody { get; set; }

    public unsafe void CodeGen(in LLVMModuleRef module, string className)
    {
        var method = new OLangMethod() {Name = MethodIdentifier.Name, ReturnType = ReturnTypeIdentifier?.ClassIdentifier.Name ?? ""};
        if (MethodParameters != null)
        {
            foreach (var parameter in MethodParameters.ParameterDeclarations)
            {
                method.Parameters.Add(new() {Class = parameter.ParameterClassName.ClassIdentifier.Name, Identifier = parameter.ParameterIdentifier.Name});
            }
        }
        OLangTypeRegistry.GetClass(className).Methods.Add(method);
        OLangTypeRegistry.CreateLLVMMethod(module, className, method);
        var entry = method.FunctionRef.AppendBasicBlock("entry");
        using var builder = module.Context.CreateBuilder();
        builder.PositionAtEnd(entry);
        //symbol table
        SymbolTable<OLangSymbol> symbolTable = new();
        var args = method.Parameters;
        symbolTable.DefineSymbol("this", new () {Class = className, TypeRef = OLangTypeRegistry.GetClass(className).ClassType, ValueRef = method.FunctionRef.GetParam(0)});
        for (int i = 0; i < args.Count; i++)
        {
            symbolTable.DefineSymbol(args[i].Identifier, new () {Class = args[i].Class, TypeRef = OLangTypeRegistry.GetClass(args[i].Class).ClassType, ValueRef = method.FunctionRef.GetParam((uint) i + 1)});
        }
        MethodBody.CodeGen(module, builder, symbolTable);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(MethodDeclaration(Name{MethodIdentifier})");
        if (MethodParameters != null)
        {
            sb.Append(MethodParameters);
        }
        if (ReturnTypeIdentifier != null)
        {
            sb.Append($"(Returns{ReturnTypeIdentifier})");
        }
        sb.Append(MethodBody);
        sb.Append(")");
        return sb.ToString();
    }
}