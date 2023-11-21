using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class Program : AstNode
{
    public List<ClassDeclaration> ProgramClasses { get; } = new List<ClassDeclaration>();

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append("(Program");
        foreach (var classDecl in ProgramClasses)
        {
            sb.Append(classDecl.ToString());
        }
        sb.Append(")");
        return sb.ToString();
    }

    public void CodeGen()
    {
        using var context = LLVMContextRef.Create();
        using var module = context.CreateModuleWithName("ProjectO module");
        using var builder = context.CreateBuilder();
        foreach (var classDecl in ProgramClasses)
        {
            classDecl.CodeGen(module, builder);
        }
        Console.WriteLine(module.PrintToString());
    }
}