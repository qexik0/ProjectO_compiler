using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public unsafe class ConstructorCall : AstNode
{
    public required ClassName ConstructorClassName { get; set; }
    public Arguments? ConstructorArguments { get; set; }

    public LLVMValueRef CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, Dictionary<string, LLVMTypeRef> types, Dictionary<string, LLVMValueRef> symbolTable)
    {
        // TODO: allocate on heap if derived from anyRef
        var res = builder.BuildAlloca(types[ConstructorClassName.ClassIdentifier.Name]);
        var constructor = module.GetNamedFunction(OLangTypeRegistry.MangleFunctionName(this));
        var args = new List<LLVMValueRef>();
        if (ConstructorArguments != null)
        {
            foreach (var argument in ConstructorArguments.Expressions)
            {
                args.Add(argument.CodeGen(module, builder, symbolTable));
            }
        }
        //hardcode
        var argTypes = new List<LLVMTypeRef>() {LLVM.PointerTypeInContext(module.Context, 0)};
        fixed (LLVMTypeRef* ptr = argTypes.ToArray())
        {
            builder.BuildCall2(LLVM.FunctionType(LLVM.VoidTypeInContext(module.Context), (LLVMOpaqueType**) ptr, 1, 0), constructor, args.ToArray());
        }
        return res;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(ConstructorCall{ConstructorClassName}");
        if (ConstructorArguments != null)
        {
            sb.Append(ConstructorArguments);
        }
        sb.Append(")");
        return sb.ToString();
    }
}