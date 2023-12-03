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
        OLangTypeRegistry.CreateLLVMConstructor(module, className, method);
        var entry = method.FunctionRef.AppendBasicBlock("entry");
        using var builder = module.Context.CreateBuilder();
        builder.PositionAtEnd(entry);
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