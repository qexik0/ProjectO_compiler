using System.Text;
using LLVMSharp.Interop;

namespace OCompiler.nodes;

public class ClassDeclaration : AstNode
{
    public required ClassName Name { get; set; }
    public ClassName? BaseClassName { get; set; }
    public List<MemberDeclaration> Members { get; } = new List<MemberDeclaration>();

    public void CodeGen(in LLVMModuleRef module, in LLVMBuilderRef builder)
    {
        LLVMTypeRef classType = module.Context.CreateNamedStruct(Name.ClassIdentifier.Name);
        List<LLVMTypeRef> memberTypes = new List<LLVMTypeRef>();

        if (BaseClassName != null)
        {
            // no idea what should happen here
        }

        // foreach (var member in Members)
        // {
        //     if (member.Member is VariableDeclaration varDecl)
        //     {

        //     }
        // }

        classType.StructSetBody(memberTypes.ToArray(), false);

        foreach (var member in Members)
        {
            if (member.Member is ConstructorDeclaration constructorCall)
            {
                constructorCall.CodeGen(classType, module);
            }
            else if (member.Member is MethodDeclaration methodDeclaration)
            {
                methodDeclaration.CodeGen(module, classType);
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"(Class{Name.ToString()}");
        if (BaseClassName != null)
        {
            sb.Append($"(extends({BaseClassName.ToString()}))");
        }
        foreach (var member in Members)
        {
            sb.Append($"{member.ToString()}");
        }
        sb.Append(")");
        return sb.ToString();
    }
}