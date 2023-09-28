namespace OCompiler.nodes;

public class Expression : AstNode
{
    public required Primary EntityPrimary { get; set; }
    public List<(Identifier, Arguments?)> Calls { get; } = new List<(Identifier, Arguments?)>();
}