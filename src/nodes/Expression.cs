using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Expression : AstNode
{
    public required AstNode PrimaryOrConstructorCall { get; set; }
    public List<(Identifier, Arguments?)> Calls { get; } = new List<(Identifier, Arguments?)>();

    public unsafe LLVMValueRef CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, SymbolTable<OLangSymbol> symbolTable)
    {
        LLVMValueRef currentVal = PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral intLiteral => intLiteral.CodeGen(module),
                RealLiteral realLiteral => realLiteral.CodeGen(module),
                BooleanLiteral booleanLiteral => booleanLiteral.CodeGen(module),
                ClassName className => symbolTable.FindSymbol(className.ClassIdentifier.Name).ValueRef, // could only be identifier
                _ => throw new ApplicationException($"Could not evaluate the expression {this}")
            },
            ConstructorCall constructorCall => constructorCall.CodeGen(module, builder, symbolTable),
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
            ConstructorCall constructorCall => constructorCall.ConstructorClassName.ClassIdentifier.Name,
            _ => throw new ApplicationException($"Could not derive type for the expression {this}")
        };
        foreach (var (identifier, call) in Calls)
        {
            var ptr = builder.BuildAlloca(currentVal.TypeOf);
            builder.BuildStore(currentVal, ptr);
            var argTypes = new List<string>();
            var args = new List<LLVMValueRef>() {ptr};
            if (call != null)
            {
                foreach (var arg in call.Expressions)
                {
                    argTypes.Add(OLangTypeRegistry.BodyExpressionType(currentType, arg, symbolTable));
                    args.Add(arg.CodeGen(module, builder, symbolTable));
                }
            }
            try
            {
                var method = OLangTypeRegistry.GetClassMethod(currentType, identifier.Name, argTypes);
                currentVal = builder.BuildCall2(method.FunctionType, method.FunctionRef, args.ToArray());
                currentType = method.ReturnType;
            }
            catch (Exception)
            {
                var indexClass = OLangTypeRegistry.GetClass(currentType);
                var index = indexClass.Fields.FindIndex(x => x.Identifier == identifier.Name);
                var indices = new LLVMValueRef[]
                {
                    LLVM.ConstInt(LLVM.Int32Type(), 0, 0), // always start with 0 for struct
                    LLVM.ConstInt(LLVM.Int32Type(), (ulong) index, 0) // index of the field
                };
                currentVal = builder.BuildInBoundsGEP2(indexClass.ClassType, ptr, indices);
                currentType = indexClass.Fields[index].Class;
            }
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