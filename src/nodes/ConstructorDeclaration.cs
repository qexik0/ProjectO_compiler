using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class ConstructorDeclaration : AstNode
{
    public Parameters? ConstructorParameters { get; set; }
    public required Body ConstructorBody { get; set; }

    public void CodeGen(in LLVMModuleRef module, string className)
    {
        var method = new OLangMethod() {Name = "", ReturnType = ""};
        if (ConstructorParameters != null)
        {
            foreach (var parameter in ConstructorParameters.ParameterDeclarations)
            {
                method.Parameters.Add(new() {Class = parameter.ParameterClassName.ClassIdentifier.Name, Identifier = parameter.ParameterIdentifier.Name});
            }
        }
        OLangTypeRegistry.GetClass(className).Methods.Add(method);
        OLangTypeRegistry.CreateLLVMConstructor(module, className, method);
        var entry = method.FunctionRef.AppendBasicBlock("entry");
        using var builder = module.Context.CreateBuilder();
        builder.PositionAtEnd(entry);
        // ConstructorBody.CodeGen(module, builder);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Constructor");
        if (ConstructorParameters != null)
        {
            sb.Append(ConstructorParameters);
        }
        sb.Append($"{ConstructorBody})");
        return sb.ToString();
    }
}