using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Expression : AstNode
{
    public required AstNode PrimaryOrConstructorCall { get; set; }
    public List<(Identifier, Arguments?)> Calls { get; } = new List<(Identifier, Arguments?)>();

    public unsafe LLVMValueRef CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, in Dictionary<string, LLVMValueRef> symbolTable)
    {
        LLVMValueRef currentVal = PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral intLiteral => intLiteral.CodeGen(module),
                RealLiteral realLiteral => realLiteral.CodeGen(module),
                BooleanLiteral booleanLiteral => booleanLiteral.CodeGen(module),
                ClassName className => symbolTable[className.ClassIdentifier.Name], // could only be identifier
                _ => throw new ApplicationException($"Could not evaluate the expression {this}")
            },
            ConstructorCall constructorCall => symbolTable[OLangTypeRegistry.MangleFunctionName(constructorCall)],
            _ => throw new ApplicationException($"Could not derive type for the expression {this}")
        };
        string currentType = PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral => "Integer",
                RealLiteral => "Real",
                BooleanLiteral => "Boolean",
                ClassName className => className.ClassIdentifier.Name,
                _ => throw new ApplicationException($"Could not derive type for the expression {this}")
            },
            ConstructorCall constructorCall => OLangTypeRegistry.MangleClassName(constructorCall.ConstructorClassName),
            _ => throw new ApplicationException($"Could not derive type for the expression {this}")
        };
        List<string> mangledArgumentTypes = new ();
        foreach (var (identifier, call) in Calls)
        {
            var ptr = builder.BuildAlloca(currentVal.TypeOf);
            var thisPtr = builder.BuildStore(currentVal, ptr);
            var args = new List<LLVMValueRef>() {thisPtr};
            if (call != null)
            {
                args.AddRange(call.CodeGen(module, builder, symbolTable));
            }
            currentVal = builder.BuildCall2(module.GetNamedFunction(OLangTypeRegistry.MangleFunctionName(currentType, identifier, )));
        }
        return currentVal;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(Expression{PrimaryOrConstructorCall}");
        foreach (var call in Calls)
        {
            sb.Append($"(Call{call.Item1}{call.Item2})");
        }
        sb.Append(")");
        return sb.ToString();
    }
}