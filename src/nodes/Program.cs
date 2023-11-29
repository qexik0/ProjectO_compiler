using System.Text;
using LLVMSharp;
using LLVMSharp.Interop;
using OCompiler.Codegen;

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
        Codegen.IntegerType.AddIntegerClass(module);
        Codegen.BooleanType.AddBooleanClass(module);
        Codegen.RealType.AddRealClass(module);
        OLangTypeRegistry.Init(module);
        foreach (var classDecl in ProgramClasses)
        {
            using var builder = module.Context.CreateBuilder();
            classDecl.CodeGen(module, builder);
        }
        Codegen.MainFunction.AddMainFunction(module, new());
        if (module.TryVerify(LLVMVerifierFailureAction.LLVMPrintMessageAction, out string message))
        {
            Console.WriteLine(message);
        }
        module.PrintToFile("output.ll");
        Console.WriteLine(module.PrintToString());
    }
}