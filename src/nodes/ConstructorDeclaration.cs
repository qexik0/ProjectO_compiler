using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class ConstructorDeclaration : AstNode
{
    public Parameters? ConstructorParameters { get; set; }
    public required Body ConstructorBody { get; set; }

    public void CodeGen(in LLVMTypeRef classType, in LLVMModuleRef module)
    {
        unsafe {
            // HERE - IF DERIVED FROM AnyValue - StructType, otherwise PointerType
            var returnType = LLVM.PointerType(classType, 0);
            var paramTypes = new List<LLVMTypeRef>();
            if (ConstructorParameters != null)
            {
                foreach (var parameter in ConstructorParameters.ParameterDeclarations)
                {
                    LLVMTypeRef parameterType = TypeRegistry.GetLLVMType(parameter.ParameterClassName.ClassIdentifier.Name);
                    paramTypes.Add(parameterType);
                }
            }
            fixed (LLVMTypeRef* paramsPtr = paramTypes.ToArray())
            {
                var constructorType = LLVM.FunctionType(returnType, (LLVMOpaqueType**) paramsPtr, (uint) paramTypes.Count, 0);
                // DO NOT FORGET TO HANDLE OVERLOADING
                var constructorFunc = module.AddFunction($"{classType.StructName}.constructor", constructorType);
                var entry = constructorFunc.AppendBasicBlock("entry");
                using var builder = module.Context.CreateBuilder();
                builder.PositionAtEnd(entry);
                // TODO: this two lines should be written in place of any return inside the constructor
                var instancePtr = builder.BuildMalloc(classType, "instance");
                builder.BuildRet(instancePtr);
                //ConstructorBody.CodeGen();
            }

        }
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