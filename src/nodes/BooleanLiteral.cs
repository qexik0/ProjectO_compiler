using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class BooleanLiteral : AstNode
{
    public required bool Value { get; set; }

    public unsafe LLVMValueRef CodeGen(in LLVMModuleRef module)
    {
        return LLVM.ConstInt(LLVM.Int1TypeInContext(module.Context), Value ? 1ul : 0ul, 0);
    }

    public override string ToString()
    {
        return $"(BooleanLiteral({Value}))";
    }
}