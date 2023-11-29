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

        // this(p: Real)
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

        // this(p: Integer)
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
        
        // method toInteger : Integer
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
        
        // method UnaryMinus : Real
        paramTypes = new[] {realPtr};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var toIntFunc = module.AddFunction("Real.UnaryMinus%%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = toIntFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = toIntFunc.GetParam(0);

            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var res = builder.BuildFNeg(thisReal, "res");
            builder.BuildRet(res);
        }
        
        // Real binary arithmetics
        // method Plus(p:Real) : Real
        paramTypes = new[] {realPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var addFunc = module.AddFunction("Real.Plus%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = addFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = addFunc.GetParam(0);
            var otherReal = addFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var sum = builder.BuildFAdd(thisReal, otherReal, "sum");
            builder.BuildRet(sum);
        }
        
        // method Plus(p:Integer) : Real
        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var addFunc = module.AddFunction("Real.Plus%Integer%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = addFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = addFunc.GetParam(0);
            var otherInt = addFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");
            
            var sum = builder.BuildFAdd(thisReal, otherReal, "sum");
            builder.BuildRet(sum);
        }
        
        // method Minus(p:Real) : Real
        paramTypes = new[] {realPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var minusFunc = module.AddFunction("Real.Minus%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = minusFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = minusFunc.GetParam(0);
            var otherReal = minusFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var difference = builder.BuildFSub(thisReal, otherReal, "difference");
            builder.BuildRet(difference);
        }
        
        // method Minus(p:Integer) : Real
        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var minusFunc = module.AddFunction("Real.Minus%Integer%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = minusFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = minusFunc.GetParam(0);
            var otherInt = minusFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");
            
            var sum = builder.BuildFSub(thisReal, otherReal, "difference");
            builder.BuildRet(sum);
        }
        
        // method Mult(p:Real) : Real
        paramTypes = new[] {realPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var multFunc = module.AddFunction("Real.Mult%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = multFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = multFunc.GetParam(0);
            var otherReal = multFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var multiplication = builder.BuildFMul(thisReal, otherReal, "multiplication");
            builder.BuildRet(multiplication);
        }
        
        // method Mult(p:Integer) : Real
        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var multFunc = module.AddFunction("Real.Mult%Integer%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = multFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = multFunc.GetParam(0);
            var otherInt = multFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");
            
            var multiplication = builder.BuildFMul(thisReal, otherReal, "difference");
            builder.BuildRet(multiplication);
        }
        
        // method Div(p:Real) : Real
        paramTypes = new[] {realPtr, realType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var divFunc = module.AddFunction("Real.Div%Real%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = divFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = divFunc.GetParam(0);
            var otherReal = divFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var quotient = builder.BuildFDiv(thisReal, otherReal, "quotient");
            builder.BuildRet(quotient);
        }
        
        // method Div(p:Integer) : Real
        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var divFunc = module.AddFunction("Real.Div%Integer%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = divFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = divFunc.GetParam(0);
            var otherInt = divFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");
            
            var quotient = builder.BuildFDiv(thisReal, otherReal, "quotient");
            builder.BuildRet(quotient);
        }
        
        // method Rem(p: Integer) : Real
        paramTypes = new[] {realPtr, intType};
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var divFunc = module.AddFunction("Real.Rem%Integer%", LLVM.FunctionType(realType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = divFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = divFunc.GetParam(0);
            var otherInt = divFunc.GetParam(1);
            
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");
            
            var reminder = builder.BuildFRem(thisReal, otherReal, "reminder");
            builder.BuildRet(reminder);
        }
        
        // Relations
        // method Less(p: Real) : Boolean
        paramTypes = new[] { realPtr, realType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessFunc = module.AddFunction("Real.Less%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = lessFunc.GetParam(0);
            var otherReal = lessFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealULT, thisReal, otherReal, "res");
            builder.BuildRet(res);
        }
        
        // method Less(p: Integer) : Boolean
        paramTypes = new[] { realPtr, intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessFunc = module.AddFunction("Real.Less%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = lessFunc.GetParam(0);
            var otherInt = lessFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealULT, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
    
        // method LessEqual(p: Real) : Boolean
        paramTypes = new[] { realPtr, realPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessEqualFunc = module.AddFunction("Real.LessEqual%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessEqualFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = lessEqualFunc.GetParam(0);
            var otherPtr = lessEqualFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildLoad2(realType, otherPtr, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealULE, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method LessEqual(p: Integer) : Boolean
        paramTypes = new[] { realPtr, intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var lessFunc = module.AddFunction("Real.LessEqual%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = lessFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = lessFunc.GetParam(0);
            var otherInt = lessFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealULE, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method Greater(p: Real) : Boolean
        paramTypes = new[] { realPtr, realPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterFunc = module.AddFunction("Real.Greater%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = greaterFunc.GetParam(0);
            var otherPtr = greaterFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildLoad2(realType, otherPtr, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGT, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method Greater(p: Integer) : Boolean
        paramTypes = new[] { realPtr, intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterFunc = module.AddFunction("Real.Greater%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = greaterFunc.GetParam(0);
            var otherInt = greaterFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGT, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method GreaterEqual(p: Real) : Boolean
        paramTypes = new[] { realPtr, realPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterEqualFunc = module.AddFunction("Real.GreaterEqual%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterEqualFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = greaterEqualFunc.GetParam(0);
            var otherPtr = greaterEqualFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildLoad2(realType, otherPtr, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGE, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        
        // method GreaterEqual(p: Integer) : Boolean
        paramTypes = new[] { realPtr, realType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var greaterFunc = module.AddFunction("Real.GreaterEqual%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = greaterFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = greaterFunc.GetParam(0);
            var otherInt = greaterFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUGE, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method Equal(p: Real) : Boolean
        paramTypes = new[] { realPtr, realPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var equalFunc = module.AddFunction("Real.Equal%Real%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = equalFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = equalFunc.GetParam(0);
            var otherPtr = equalFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");
            var otherReal = builder.BuildLoad2(realType, otherPtr, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
        
        // method Equal(p: Integer) : Boolean
        paramTypes = new[] { realPtr, intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var equalFunc = module.AddFunction("Real.Equal%Integer%", LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = equalFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);

            var thisPtr = equalFunc.GetParam(0);
            var otherInt = equalFunc.GetParam(1);
            var thisReal = builder.BuildLoad2(realType, thisPtr, "thisReal");

            // Convert the input integer to a floating-point value
            var otherReal = builder.BuildSIToFP(otherInt, realType, "otherReal");

            var res = builder.BuildFCmp(LLVMRealPredicate.LLVMRealUEQ, thisReal, otherReal, "res");

            builder.BuildRet(res);
        }
    }
}