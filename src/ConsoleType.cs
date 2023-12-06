using LLVMSharp.Interop;
using static OCompiler.Codegen.OLangTypeRegistry;

namespace OCompiler.Codegen;

public unsafe static class ConsoleType
{
    public static void AddConsoleClass(in LLVMModuleRef module)
    {        
        var intType = LLVM.Int32TypeInContext(module.Context);
        var realType = LLVM.DoubleTypeInContext(module.Context);
        var boolType = LLVM.Int1TypeInContext(module.Context);
        var consoleType = LLVM.Int32TypeInContext(module.Context);
        var consolePtr = LLVM.PointerTypeInContext(module.Context, 0);
        var voidType = LLVM.VoidTypeInContext(module.Context);

        LLVMValueRef printingIntFunc, printingRealFunc, printingBoolFunc;
        LLVMTypeRef printingIntType, printingRealType, printingBoolType;

        var paramTypes = new[] { intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            printingIntType = LLVM.FunctionType(voidType, prms, 1, 0);
            printingIntFunc = module.AddFunction("printInt", printingIntType);
        }

        paramTypes = new[] { realType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            printingRealType = LLVM.FunctionType(voidType, prms, 1, 0);
            printingRealFunc = module.AddFunction("printReal", printingRealType);
        }

        paramTypes = new[] { boolType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            printingBoolType = LLVM.FunctionType(voidType, prms, 1, 0);
            printingBoolFunc = module.AddFunction("printBool", printingBoolType);
        }

        // method printInt
        paramTypes = new[] { consolePtr, intType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var printInt = module.AddFunction("Console%Integer%",
                LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = printInt.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Console").Methods.Add(new OLangMethod()
            {
                Name = "", 
                FunctionType = LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = printInt,
                ReturnType = "",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Integer", Identifier = "arg0"
                    }
                }
            });

            var intParam = printInt.GetParam(1);
            builder.BuildCall2(printingIntType, printingIntFunc, new[] {intParam});
            builder.BuildRetVoid();
        }

        // method printReal
        paramTypes = new[] { consolePtr, realType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var printReal = module.AddFunction("Console%Real%",
                LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = printReal.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Console").Methods.Add(new OLangMethod()
            {
                Name = "", 
                FunctionType = LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = printReal,
                ReturnType = "",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Real", Identifier = "arg0"
                    }
                }
            });

            var realParam = printReal.GetParam(1);
            builder.BuildCall2(printingRealType, printingRealFunc, new[] {realParam});
            builder.BuildRetVoid();
        }

        // method printBool
        paramTypes = new[] { consolePtr, boolType };
        fixed (LLVMOpaqueType** prms = paramTypes)
        {
            var printBool = module.AddFunction("Console%Boolean%",
                LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0));
            var context = module.Context;
            using var builder = context.CreateBuilder();
            var entry = printBool.AppendBasicBlock("entry");
            builder.PositionAtEnd(entry);
            
            GetClass("Console").Methods.Add(new OLangMethod()
            {
                Name = "", 
                FunctionType = LLVM.FunctionType(voidType, prms, (uint)paramTypes.Length, 0),
                FunctionRef = printBool,
                ReturnType = "",
                Parameters = new ()
                {
                    new ()
                    {
                        Class = "Boolean", Identifier = "arg0"
                    }
                }
            });

            var realParam = printBool.GetParam(1);
            builder.BuildCall2(printingBoolType, printingBoolFunc, new[] {realParam});
            builder.BuildRetVoid();
        }
    }
}