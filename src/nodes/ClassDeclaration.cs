namespace OCompiler.nodes;

public class ClassDeclaration : AstNode
{
    public required string ClassName { get; set; }
    public string? BaseClassName { get; set; }
    public List<MemberDeclaration> Members { get; } = new List<MemberDeclaration>();
}