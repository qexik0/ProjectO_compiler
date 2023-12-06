using LLVMSharp.Interop;
using static OCompiler.Codegen.OLangTypeRegistry;

namespace OCompiler.Codegen;

public unsafe static class BooleanType
{
    public static void AddBooleanClass(in LLVMModuleRef module)
    {
        var intType = LLVM.Int32TypeInContext(module.Context);
        var boolType = LLVM.Int1TypeInContext(module.Context);
        var boolPtr = LLVM.PointerTypeInContext(module.Context, 0);

        // method Or
        var paramTypes = new[] { boolPtr, boolType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var orFunc = module.AddFunction("Boolean.Or%Boolean%",
                LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            LLVM.SetFunctionCallConv(orFunc, (uint)LLVMCallConv.LLVMCCallConv);
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = orFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Boolean").Methods.Add(new OLangMethod()
            {
                Name = "Or", 
                FunctionType = LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = orFunc,
                ReturnType = "Boolean",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Boolean", Identifier = "arg0"
                    }
                }
            });

            var thisPtr = orFunc.GetParam(0);
            var otherBoolean = orFunc.GetParam(1);

            var thisBoolean = builder.BuildLoad2(boolType, thisPtr, "thisBoolean");
            var orResult = builder.BuildOr(thisBoolean, otherBoolean, "Or");
            builder.BuildRet(orResult);
        }
        
        // method And
        paramTypes = new[] { boolPtr, boolType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var andFunc = module.AddFunction("Boolean.And%Boolean%",
                LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            LLVM.SetFunctionCallConv(andFunc, (uint)LLVMCallConv.LLVMCCallConv);
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = andFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Boolean").Methods.Add(new OLangMethod()
            {
                Name = "And", 
                FunctionType = LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = andFunc,
                ReturnType = "Boolean",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Boolean", Identifier = "arg0"
                    }
                }
            });

            var thisPtr = andFunc.GetParam(0);
            var otherBoolean = andFunc.GetParam(1);

            var thisBoolean = builder.BuildLoad2(boolType, thisPtr, "thisBoolean");
            var andResult = builder.BuildAnd(thisBoolean, otherBoolean, "And");
            builder.BuildRet(andResult);
        }
        
        // method Xor
        paramTypes = new[] { boolPtr, boolType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var xorFunc = module.AddFunction("Boolean.Xor%Boolean%",
                LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            LLVM.SetFunctionCallConv(xorFunc, (uint)LLVMCallConv.LLVMCCallConv);
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = xorFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Boolean").Methods.Add(new OLangMethod()
            {
                Name = "Xor", 
                FunctionType = LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = xorFunc,
                ReturnType = "Boolean",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Boolean", Identifier = "arg0"
                    }
                }
            });

            var thisPtr = xorFunc.GetParam(0);
            var otherBoolean = xorFunc.GetParam(1);

            var thisBoolean = builder.BuildLoad2(boolType, thisPtr, "thisBoolean");
            var xorResult = builder.BuildAnd(thisBoolean, otherBoolean, "Xor");
            builder.BuildRet(xorResult);
        }
        
        // method Not
        paramTypes = new[] { boolPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var notFunc = module.AddFunction("Boolean.Not%%",
                LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0));
            LLVM.SetFunctionCallConv(notFunc, (uint)LLVMCallConv.LLVMCCallConv);
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = notFunc.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Boolean").Methods.Add(new OLangMethod()
            {
                Name = "Not", 
                FunctionType = LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = notFunc,
                ReturnType = "Boolean"
            });

            var thisPtr = notFunc.GetParam(0);

            var thisBoolean = builder.BuildLoad2(boolType, thisPtr, "thisBoolean");
            var thisNot = builder.BuildNot(thisBoolean);
            builder.BuildRet(thisNot);
        }
        
        // method toInteger
        paramTypes = new[] { boolPtr };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var toInteger = module.AddFunction("Boolean.toInteger%%",
                LLVM.FunctionType(intType, prms, (uint)paramTypes.Length, 0));
            LLVM.SetFunctionCallConv(toInteger, (uint)LLVMCallConv.LLVMCCallConv);
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = toInteger.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Boolean").Methods.Add(new OLangMethod()
            {
                Name = "toInteger", 
                FunctionType = LLVM.FunctionType(boolType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = toInteger,
                ReturnType = "Boolean"
            });

            var thisPtr = toInteger.GetParam(0);

            var thisBoolean = builder.BuildLoad2(boolType, thisPtr, "thisBoolean");
            
            var zero = LLVM.ConstInt(intType, 0, 0);
            var one = LLVM.ConstInt(intType, 1, 0);

            var integerResult = builder.BuildSelect(thisBoolean, one, zero, "integerResult");
            builder.BuildRet(integerResult);
        }
    }
}