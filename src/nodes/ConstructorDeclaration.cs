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
        OLangTypeRegistry.AddMethod(className, this);
        var methodName = OLangTypeRegistry.MangleFunctionName(className, this);
        var methodType = OLangTypeRegistry.GetLLVMMethodType(className, this);
        var method = module.AddFunction(methodName, methodType);
        var entry = method.AppendBasicBlock("entry");
        using var builder = module.Context.CreateBuilder();
        builder.PositionAtEnd(entry);
        ConstructorBody.CodeGen(module, builder);
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