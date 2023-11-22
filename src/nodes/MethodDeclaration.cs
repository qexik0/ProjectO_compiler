using System.Reflection.Metadata;
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
                // DO NOT FORGET TO HANDLE GENERICS
                returnType = TypeRegistry.GetLLVMType(ReturnTypeIdentifier.ClassIdentifier.Name);
            }
            
            // DON'T FORGET TO ADD THE FIRST ARGUMENT - THIS POINTER
            var paramTypes = new List<LLVMTypeRef>
            {
                LLVM.PointerType(classType, 0)
            };
            if (MethodParameters != null)
            {
                foreach (var parameter in MethodParameters.ParameterDeclarations)
                {
                    LLVMTypeRef parameterType = TypeRegistry.GetLLVMType(parameter.ParameterClassName.ClassIdentifier.Name);
                    paramTypes.Add(parameterType);
                }
            }
            fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
            {
                var methodType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
                //DON'T FORGET TO HANDLE OVERLOADING
                var method = module.AddFunction($"{classType.StructName}.{MethodIdentifier.Name}", methodType);
                var entry = method.AppendBasicBlock("entry");
                using var builder = module.Context.CreateBuilder();
                builder.PositionAtEnd(entry);
                //MethodBody.CodeGen();
            }
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