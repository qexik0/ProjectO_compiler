namespace OCompiler.nodes;

public class Program : AstNode
{
    public List<ClassDeclaration> ProgramClasses { get; } = new List<ClassDeclaration>();
}