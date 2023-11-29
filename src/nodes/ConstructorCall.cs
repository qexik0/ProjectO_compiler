using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class ConstructorCall : AstNode
{
    public required ClassName ConstructorClassName { get; set; }
    public Arguments? ConstructorArguments { get; set; }

    public LLVMValueRef CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, Dictionary<string, LLVMTypeRef> types, Dictionary<string, LLVMValueRef> symbolTable)
    {
        // TODO: allocate on heap if derived from anyRef
        var res = builder.BuildAlloca(types[ConstructorClassName.ClassIdentifier.Name]);
        var constructor = symbolTable[OLangTypeRegistry.MangleFunctionName(this)];
        var args = new List<LLVMValueRef>();
        if (ConstructorArguments != null)
        {
            foreach (var argument in ConstructorArguments.Expressions)
            {
                args.Add(argument.CodeGen(module, symbolTable));
            }
        }
        builder.BuildCall2(types[OLangTypeRegistry.MangleFunctionName(this)], constructor, args.ToArray());
        return res;
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