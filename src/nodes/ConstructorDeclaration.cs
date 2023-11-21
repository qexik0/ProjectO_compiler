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
            //here the types of arguments and pass them to FunctionType.
            //var paramTypes = Array.Empty<LLVMTypeRef>();
            var constructorType = LLVM.FunctionType(LLVM.PointerType(classType, 0), null, 0, 0);
            var constructorFunc = module.AddFunction($"{classType.StructName}.constructor", constructorType);

            var entry = constructorFunc.AppendBasicBlock("entry");
            using var builder = module.Context.CreateBuilder();
            builder.PositionAtEnd(entry);

            var instancePtr = builder.BuildMalloc(classType, "instance");

            builder.BuildRet(instancePtr);
            //ConstructorBody.CodeGen();
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