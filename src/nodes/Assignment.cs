namespace OCompiler.nodes;

public class Assignment
{
    public required Identifier AssignmentIdentifier { get; set; }
    public required Expression AssignmentExpression { get; set; }
}