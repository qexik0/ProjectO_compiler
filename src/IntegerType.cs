using LLVMSharp.Interop;

namespace OCompiler.Codegen;

public unsafe static class IntegerType
{
    public static Dictionary<String, LLVMValueRef> mapping = new Dictionary<string, LLVMValueRef>();  
    public static void AddIntegerClass(in LLVMModuleRef module)
    {
        var intType = LLVM.Int32TypeInContext(module.Context);
        var realType = LLVM.DoubleTypeInContext(module.Context);
        var intPtr = LLVM.PointerTypeInContext(module.Context, 0);
        var boolType = LLVM.Int1TypeInContext(module.Context);
        var voidType = LLVM.VoidTypeInContext(module.Context);

        var paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var addFunc = module.AddFunction("Integer.Plus%Integer%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = addFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            mapping["Integer.Plus%Integer%"] = addFunc;

            var thisPtr = addFunc.GetParam(0);
            var otherInt = addFunc.GetParam(1);
            
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var sum = builder.BuildAdd(thisInt, otherInt, "sum");
            builder.BuildRet(sum);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var addFunc = module.AddFunction("Integer.Plus%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = addFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Plus%Real%"] = addFunc;

            var thisPtr = addFunc.GetParam(0);
            var otherReal = addFunc.GetParam(1);
            
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");

            var sum = builder.BuildFAdd(thisReal, otherReal, "sum");
            builder.BuildRet(sum);
        }

        paramTypes = new[] {intPtr};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var negFunc = module.AddFunction("Integer.UnaryMinus%%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = negFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.UnaryMinus%%"] = negFunc;

            var thisPtr = negFunc.GetParam(0);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisNeg = builder.BuildNeg(thisInt, "thisNeg");
            builder.BuildRet(thisNeg);
        }

        paramTypes = new[] {intPtr};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var toRealFunc = module.AddFunction("Integer.ToReal%%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = toRealFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.ToReal%%"] = toRealFunc;

            var thisPtr = toRealFunc.GetParam(0);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            builder.BuildRet(thisReal);
        }

        paramTypes = new[] {intPtr};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var toBoolFunc = module.AddFunction("Integer.ToBoolean%%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = toBoolFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.ToBoolean%%"] = toBoolFunc;

            var thisPtr = toBoolFunc.GetParam(0);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var zero = LLVM.ConstInt(intType, 0, 0);
            var isNonZero = builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, thisInt, zero, "isNonZero");
            builder.BuildRet(isNonZero);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var intConstructor = module.AddFunction("Integer%Integer%", LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = intConstructor.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer%Integer%"] = intConstructor;

            var thisPtr = intConstructor.GetParam(0);
            var otherInt = intConstructor.GetParam(1);
            builder.BuildStore(otherInt, thisPtr);
            builder.BuildRetVoid();
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var realConstructor = module.AddFunction("Integer%Real%", LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = realConstructor.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer%Real%"] = realConstructor;

            var thisPtr = realConstructor.GetParam(0);
            var otherReal = realConstructor.GetParam(1);
            var res = builder.BuildFPToSI(otherReal, intType, "res");
            builder.BuildStore(res, thisPtr);
            builder.BuildRetVoid();
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var minusFunc = module.AddFunction("Integer.Minus%Integer%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = minusFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Minus%Integer%"] = minusFunc;

            var thisPtr = minusFunc.GetParam(0);
            var otherInt = minusFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildSub(thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var minusFunc = module.AddFunction("Integer.Minus%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = minusFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Minus%Real%"] = minusFunc;

            var thisPtr = minusFunc.GetParam(0);
            var otherReal = minusFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFSub(thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var multFunc = module.AddFunction("Integer.Mult%Integer%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = multFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Mult%Integer%"] = multFunc;

            var thisPtr = multFunc.GetParam(0);
            var otherInt = multFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildMul(thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var multFunc = module.AddFunction("Integer.Mult%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = multFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Mult%Real%"] = multFunc;

            var thisPtr = multFunc.GetParam(0);
            var otherReal = multFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "res");
            var res = builder.BuildFMul(thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var divFunc = module.AddFunction("Integer.Div%Integer%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = divFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Div%Integer%"] = divFunc;

            var thisPtr = divFunc.GetParam(0);
            var otherInt = divFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildSDiv(thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var divFunc = module.AddFunction("Integer.Div%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = divFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Div%Real%"] = divFunc;

            var thisPtr = divFunc.GetParam(0);
            var otherReal = divFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFDiv(thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var remFunc = module.AddFunction("Integer.Rem%Integer%", LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = remFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Rem%Integer%"] = remFunc;

            var thisPtr = remFunc.GetParam(0);
            var otherInt = remFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildSRem(thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessFunc = module.AddFunction("Integer.Less%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Less%Integer%"] = lessFunc;

            var thisPtr = lessFunc.GetParam(0);
            var otherInt = lessFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessEqual = module.AddFunction("Integer.LessEqual%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessEqual.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.LessEqual%Integer%"] = lessEqual;

            var thisPtr = lessEqual.GetParam(0);
            var otherInt = lessEqual.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildICmp(LLVMIntPredicate.LLVMIntSLE, thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterFunc = module.AddFunction("Integer.Greater%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Greater%Integer%"] = greaterFunc;

            var thisPtr = greaterFunc.GetParam(0);
            var otherInt = greaterFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildICmp(LLVMIntPredicate.LLVMIntSGT, thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterEqual = module.AddFunction("Integer.GreaterEqual%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterEqual.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.GreaterEqual%Integer%"] = greaterEqual;

            var thisPtr = greaterEqual.GetParam(0);
            var otherInt = greaterEqual.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildICmp(LLVMIntPredicate.LLVMIntSGE, thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var equalFunc = module.AddFunction("Integer.Equal%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = equalFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Equal%Integer%"] = equalFunc;

            var thisPtr = equalFunc.GetParam(0);
            var otherInt = equalFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var res = builder.BuildICmp(LLVMIntPredicate.LLVMIntEQ, thisInt, otherInt, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessFunc = module.AddFunction("Integer.Less%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Less%Real%"] = lessFunc;

            var thisPtr = lessFunc.GetParam(0);
            var otherReal = lessFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLT, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessEqual = module.AddFunction("Integer.LessEqual%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessEqual.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.LessEqual%Real%"] = lessEqual;

            var thisPtr = lessEqual.GetParam(0);
            var otherReal = lessEqual.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealOLE, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterFunc = module.AddFunction("Integer.Greater%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Greater%Real%"] = greaterFunc;

            var thisPtr = greaterFunc.GetParam(0);
            var otherReal = greaterFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGT, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterEqual = module.AddFunction("Integer.GreaterEqual%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterEqual.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.GreaterEqual%Real%"] = greaterEqual;

            var thisPtr = greaterEqual.GetParam(0);
            var otherReal = greaterEqual.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealOGE, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }

        paramTypes = new[] {intPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var equalFunc = module.AddFunction("Integer.Equal%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = equalFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            mapping["Integer.Equal%Real%"] = equalFunc;

            var thisPtr = equalFunc.GetParam(0);
            var otherReal = equalFunc.GetParam(1);
            var thisInt = builder.BuildLoad2(intType, thisPtr, "thisInt");
            var thisReal = builder.BuildSIToFP(thisInt, realType, "thisReal");
            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealOEQ, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }
    }
}