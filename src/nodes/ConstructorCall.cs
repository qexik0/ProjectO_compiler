using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public unsafe class ConstructorCall : AstNode
{
    public required ClassName ConstructorClassName { get; set; }
    public Arguments? ConstructorArguments { get; set; }

    public LLVMValueRef CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, SymbolTable<OLangSymbol> symbolTable)
    {
        // TODO: allocate on heap if derived from anyRef
        var res = builder.BuildAlloca(OLangTypeRegistry.GetClass(ConstructorClassName.ClassIdentifier.Name).ClassType);
        var args = new List<LLVMValueRef>() {res};
        var olangArgs = new List<string>();
        if (ConstructorArguments != null)
        {
            args.AddRange(ConstructorArguments.CodeGen(module, builder, symbolTable));
            foreach (var arg in ConstructorArguments.Expressions)
            {
                olangArgs.Add(OLangTypeRegistry.BodyExpressionType(ConstructorClassName.ClassIdentifier.Name, arg, symbolTable));
            }
        }
        var constructor = OLangTypeRegistry.GetClassMethod(ConstructorClassName.ClassIdentifier.Name, "", olangArgs);
        builder.BuildCall2(constructor.FunctionType, constructor.FunctionRef, args.ToArray());
        var resVal = builder.BuildLoad2(OLangTypeRegistry.GetClass(ConstructorClassName.ClassIdentifier.Name).ClassType, res);
        return resVal;
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