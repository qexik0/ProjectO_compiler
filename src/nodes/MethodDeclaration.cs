using System.Text;
using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class MethodDeclaration : AstNode
{
    public required Identifier MethodIdentifier { get; set; }
    public Parameters? MethodParameters { get; set; }
    public ClassName? ReturnTypeIdentifier { get; set; }
    public required Body MethodBody { get; set; }

    public void CodeGen(in LLVMModuleRef module, in LLVMTypeRef classType)
    {
        unsafe {
            LLVMOpaqueType* returnType = LLVM.VoidType();
            if (ReturnTypeIdentifier != null)
            {
                // here we need to actually do something, but for now hardcoding it to Integer.
                returnType = LLVM.Int32Type();
            }

            //should codegen arguments, DON'T FORGET - THE FIRST ARGUMENT IS THIS POINTER
            //var paramTypes = Array.Empty<LLVMTypeRef>();
            var methodType = LLVM.FunctionType(returnType, null, 0, 0);
            var method = module.AddFunction($"{classType.StructName}.{MethodIdentifier.Name}", methodType);

            var entry = method.AppendBasicBlock("entry");
            using var builder = module.Context.CreateBuilder();
            builder.PositionAtEnd(entry);
            
            //MethodBody.CodeGen();

        }
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