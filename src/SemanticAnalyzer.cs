using System.Text;
using OCompiler.nodes;
using Table = System.Collections.Generic.Dictionary<string, string>;

namespace OCompiler;

public class SemanticAnalyzer
{
    private Program _root;
    private List<Token> _tokens;

    public SemanticAnalyzer(Program root, List<Token> tokens)
    {
        _root = root;
        _tokens = tokens;
    }

    public void AnalyzeProgram()
    {
        foreach (var declaration in _root.ProgramClasses)
        {
            AnalyzeClass(declaration);
        }
    }

    private void AnalyzeClass(ClassDeclaration declaration)
    {
        var (variables, methods, constructors) = GetMembersWithTypes(declaration);
    }

    private (List<Variable>, List<Method>, List<Constructor>) GetMembersWithTypes(ClassDeclaration declaration)
    {
        var variables = new List<Variable>();
        var methods = new List<Method>();
        var constructors = new List<Constructor>();

        foreach (var member in declaration.Members)
        {
            switch (member.Member)
            {
                case VariableDeclaration:
                    var variableDeclaration = (VariableDeclaration)member.Member;
                    var varName = variableDeclaration.VariableIdentifier.Name;
                    var className =
                        (ClassName)((Primary)variableDeclaration.VariableExpression.PrimaryOrConstructorCall).Node;
                    var type = GetTypeFromClassName(className);
                    var variable = new Variable { Name = varName, Type = type };
                    variables.Add(variable);
                    break;
                case MethodDeclaration:
                    var methodDeclaration = (MethodDeclaration)member.Member;
                    var methodName = methodDeclaration.MethodIdentifier.Name;
                    var returnType = methodDeclaration.ReturnTypeIdentifier;
                    var methodType = returnType == null ? "Void" : GetTypeFromClassName(returnType);

                    var method = new Method { Name = methodName, ReturnType = methodType };
                    if (methodDeclaration.MethodParameters != null)
                    {
                        method.Parameters.AddRange(methodDeclaration.MethodParameters.ParameterDeclarations.Select(
                            parameter =>
                                GetTypeFromClassName(parameter.ParameterClassName)));
                    }

                    methods.Add(method);
                    break;
                case ConstructorDeclaration:
                    var constructorDeclaration = (ConstructorDeclaration)member.Member;
                    var constructor = new Constructor { Type = GetTypeFromClassName(declaration.Name) };
                    if (constructorDeclaration.ConstructorParameters != null)
                    {
                        constructor.Parameters.AddRange(
                            constructorDeclaration.ConstructorParameters.ParameterDeclarations.Select(parameter =>
                                GetTypeFromClassName(parameter.ParameterClassName)));
                    }

                    constructors.Add(constructor);
                    break;
            }
        }

        return (variables, methods, constructors);
    }

    private string GetTypeFromClassName(ClassName className)
    {
        var type = new StringBuilder(className.ClassIdentifier.Name);
        var genericCount = 0;
        while (className.GenericClassName != null)
        {
            className = className.GenericClassName!;
            type.Append($"[{className.ClassIdentifier.Name}");
            genericCount++;
        }

        var brackets = new StringBuilder(genericCount).Insert(0, "]", genericCount).ToString();
        type.Append(brackets);
        return type.ToString();
    }
}

class Variable : IEquatable<Variable>
{
    public string Name { get; init; }
    public string Type { get; init; }

    public bool Equals(Variable? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Name.Equals(other.Name) && Type.Equals(other.Type);
    }
}

class Method : IEquatable<Method>
{
    public string Name { get; init; }
    public string ReturnType { get; init; }
    public List<string> Parameters { get; } = new();

    public bool Equals(Method? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Name.Equals(other.Name) && ReturnType.Equals(other.ReturnType) &&
               Parameters.SequenceEqual(other.Parameters);
    }
}

class Constructor : IEquatable<Constructor>
{
    public string Type { get; init; }
    public List<string> Parameters { get; } = new();


    public bool Equals(Constructor? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Type.Equals(other.Type) && Parameters.SequenceEqual(other.Parameters);
    }
}