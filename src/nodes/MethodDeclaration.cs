using System.Reflection.Metadata;
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
        // OLangTypeRegistry.AddMethod(className, this);
        // var methodName = OLangTypeRegistry.MangleFunctionName(className, this);
        // var methodType = OLangTypeRegistry.GetLLVMMethodType(className, this);
        // var method = module.AddFunction(methodName, methodType);
        // var entry = method.AppendBasicBlock("entry");
        // using var builder = module.Context.CreateBuilder();
        // builder.PositionAtEnd(entry);
        // //MethodBody.Codegen();
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