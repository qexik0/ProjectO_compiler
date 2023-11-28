using LLVMSharp.Interop;

namespace OCompiler.Codegen;

public unsafe static class RealType
{
    public static void AddRealClass(in LLVMModuleRef module)
    {
        var intType = LLVM.Int32TypeInContext(module.Context);
        var realType = LLVM.DoubleTypeInContext(module.Context);
        var realPtr = LLVM.PointerTypeInContext(module.Context, 0);
        var boolType = LLVM.Int1TypeInContext(module.Context);
        var voidType = LLVM.VoidTypeInContext(module.Context);

        var paramTypes = new[] {realPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var realConstructor = module.AddFunction("Real%Real%", LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = realConstructor.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = realConstructor.GetParam(0);
            var otherReal = realConstructor.GetParam(1);
            
            builder.BuildStore(otherReal, thisPtr);
            builder.BuildRetVoid();
        }

        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var intConstructor = module.AddFunction("Real%Integer%", LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = intConstructor.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = intConstructor.GetParam(0);
            var otherInt = intConstructor.GetParam(1);

            var res = builder.BuildSIToFP(otherInt, realType, "res");
            builder.BuildStore(res, thisPtr);
            builder.BuildRetVoid();
        }

        paramTypes = new[] {realPtr};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var toIntFunc = module.AddFunction("Real.toInteger%%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = toIntFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = toIntFunc.GetParam(0);

            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var res = builder.BuildFPToSI(thisReal, intType, "res");
            builder.BuildRet(res);
        }
    }
}