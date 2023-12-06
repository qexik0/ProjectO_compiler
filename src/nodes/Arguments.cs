using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class Arguments : AstNode
{
    public List<Expression> Expressions { get; } = new List<Expression>();

    public unsafe List<LLVMValueRef> CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder, SymbolTable<OLangSymbol> symbolTable)
    {
        var args = new List<LLVMValueRef>();
        foreach (var expr in Expressions)
        {
            args.Add(expr.CodeGen(module, builder, symbolTable));
        }
        return args;
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Arguments");
        foreach (var expr in Expressions)
        {
            sb.Append(expr);
        }
        sb.Append(")");
        return sb.ToString();
    }
}