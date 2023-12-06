using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class RealLiteral : AstNode
{
    public required double Value { get; set; }

    public unsafe LLVMValueRef CodeGen(in LLVMModuleRef module)
    {
        return LLVM.ConstReal(LLVM.DoubleTypeInContext(module.Context), Value);
    }

    public override string ToString()
    {
        return $"(RealLiteral({Value}))";
    }
}