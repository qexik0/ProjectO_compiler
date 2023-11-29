using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class IntegerLiteral : AstNode
{
    public required int Value { get; set; }

    public unsafe LLVMValueRef CodeGen(in LLVMModuleRef module)
    {
        return LLVM.ConstInt(LLVM.Int32TypeInContext(module.Context), (ulong)Value, 0);
    }

    public override string ToString()
    {
        return $"(IntegerLiteral({Value}))";
    }
}