namespace OCompiler.nodes;

public class ClassDeclaration : AstNode
{
    public required ClassName Name { get; set; }
    public ClassName? BaseClassName { get; set; }
    public List<MemberDeclaration> Members { get; } = new List<MemberDeclaration>();
}