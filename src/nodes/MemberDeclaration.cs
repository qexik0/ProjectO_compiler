namespace OCompiler.nodes;

public class MemberDeclaration : AstNode
{
    public required AstNode Member { get; set; }
}