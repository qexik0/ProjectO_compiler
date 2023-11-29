using LLVMSharp.Interop;

namespace OCompiler.Codegen;

public unsafe static class MainFunction
{
    public static void AddMainFunction(in LLVMModuleRef module, in Dictionary<string, LLVMValueRef> symbolTable)
    {
        // TODO: get all the constructor classes and map them against class name
        // take argc and argv and generate entry point
        // right now calls Program constructor without arguments
        var intType = LLVM.Int32TypeInContext(module.Context);
        var mainFuncType = LLVM.FunctionType(intType, null, 0, 0);
        var mainFunc = module.AddFunction("main", mainFuncType);

        var entry = mainFunc.AppendBasicBlock("entry");
        using var builder = module.Context.CreateBuilder();
        builder.PositionAtEnd(entry);

        var str = builder.BuildAlloca(module.GetTypeByName("Program"));
        var args = new List<LLVMValueRef>() {str};
        var arr = new List<LLVMTypeRef>() {LLVM.PointerTypeInContext(module.Context, 0)};
        fixed (LLVMTypeRef* fa = arr.ToArray())
        {
            var type = LLVM.FunctionType(LLVM.VoidTypeInContext(module.Context), (LLVMOpaqueType**) fa, 1, 0);
            builder.BuildCall2(type, module.GetNamedFunction("Program%%"), args.ToArray());
            builder.BuildRet(LLVM.ConstInt(intType, 0, 0));
        }
        
    }
}