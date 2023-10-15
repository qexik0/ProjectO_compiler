namespace OCompiler.nodes;

public class Identifier : AstNode
{
    public required string Name { get; set; }

    public override string ToString()
    {
        return $"(Identifer({Name}))";
    }
}