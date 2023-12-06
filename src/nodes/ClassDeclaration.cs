using System.Text;
using LLVMSharp.Interop;
using OCompiler.Codegen;

namespace OCompiler.nodes;

public class ClassDeclaration : AstNode
{
    public required ClassName Name { get; set; }
    public ClassName? BaseClassName { get; set; }
    public List<MemberDeclaration> Members { get; } = new List<MemberDeclaration>();

    public void CodeGen(in LLVMModuleRef module)
    {
        if (BaseClassName != null)
        {
            // init a vtable, copy members of base class into type registry
        }
        // TODO: handle generics
        var className = Name.ClassIdentifier.Name;
        OLangTypeRegistry.Types.Add(new() {Identifier = className});
        foreach (var member in Members)
        {
            if (member.Member is VariableDeclaration varDecl)
            {
                var fieldType = OLangTypeRegistry.FieldExpressionType(className, varDecl.VariableExpression);
                OLangTypeRegistry.GetClass(className).Fields.Add(new() {Class = fieldType, Identifier = varDecl.VariableIdentifier.Name});
            }
        }
        OLangTypeRegistry.CreateLLVMType(module, className);

        foreach (var member in Members)
        {
            if (member.Member is ConstructorDeclaration constructorDeclaration)
            {
                constructorDeclaration.CodeGen(module, className);
            }
        }

        foreach (var member in Members)
        {
            if (member.Member is MethodDeclaration methodDeclaration)
            {
                methodDeclaration.CodeGen(module, className);
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