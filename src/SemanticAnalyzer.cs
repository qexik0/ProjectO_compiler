using System.Text;
using OCompiler.nodes;
using Table = System.Collections.Generic.Dictionary<string, string>;

namespace OCompiler;

public class SemanticAnalyzer
{
    private Program _root;
    private List<Token> _tokens;
    private Dictionary<string, CurrentClass> classes;

    public SemanticAnalyzer(Program root, List<Token> tokens)
    {
        _root = root;
        _tokens = tokens;
        classes = CollectClasses(_root);
    }

    public void AnalyzeProgram()
    {
        foreach (var declaration in _root.ProgramClasses)
        {
            AnalyzeClass(declaration);
        }
    }

    private Dictionary<string, CurrentClass> CollectClasses(Program root)
    {
        Dictionary<string, CurrentClass> allClasses = new();
        foreach (var classDeclaration in root.ProgramClasses)
        {
            var curClass = new CurrentClass()
            {
                Name = classDeclaration.Name.ClassIdentifier.Name,
                BaseClass = classDeclaration.BaseClassName == null
                    ? null
                    : GetTypeFromClassName(classDeclaration.BaseClassName),
                Generic = classDeclaration.Name.GenericClassName?.ClassIdentifier.Name
            };
            foreach (var member in classDeclaration.Members)
            {
                switch (member.Member)
                {
                    case MethodDeclaration methodDeclaration:
                        var tmpParameters = new List<Variable>();
                        if (methodDeclaration.MethodParameters != null)
                        {
                            foreach (var parameter in methodDeclaration.MethodParameters.ParameterDeclarations)
                            {
                                tmpParameters.Add(new Variable
                                {
                                    Name = parameter.ParameterIdentifier.Name,
                                    Type = GetTypeFromClassName(parameter.ParameterClassName)
                                });
                            }
                        }

                        var method = new Method
                        {
                            Name = CreateName(methodDeclaration.MethodIdentifier.Name, tmpParameters),
                            MethodBody = methodDeclaration.MethodBody,
                            ReturnType = methodDeclaration.ReturnTypeIdentifier == null
                                ? "Void"
                                : GetTypeFromClassName(methodDeclaration.ReturnTypeIdentifier)
                        };

                        foreach (var parameter in tmpParameters)
                        {
                            method.Parameters.Add(parameter.Type, parameter);
                        }

                        curClass.Methods.Add(method.Name, method);
                        break;
                    case VariableDeclaration variableDeclaration:
                        var primary = (Primary)variableDeclaration.VariableExpression.PrimaryOrConstructorCall;
                        var variable = new Variable
                        {
                            Name = variableDeclaration.VariableIdentifier.Name,
                            Type = GetTypeFromClassName((ClassName)primary.Node)
                        };
                        curClass.Variables.Add(variable.Name, variable);
                        break;
                    case ConstructorDeclaration constructorDeclaration:
                        var tempParams = new List<Variable>();
                        if (constructorDeclaration.ConstructorParameters != null)
                        {
                            foreach (var parameter in constructorDeclaration.ConstructorParameters
                                         .ParameterDeclarations)
                            {
                                tempParams.Add(new Variable
                                {
                                    Name = parameter.ParameterIdentifier.Name,
                                    Type = GetTypeFromClassName(parameter.ParameterClassName)
                                });
                            }
                        }

                        // TODO: decide about class name
                        var constructor = new Constructor
                        {
                            Name = CreateName("", tempParams),
                            Type = curClass.Name,
                            ConstructorBody = constructorDeclaration.ConstructorBody
                        };

                        foreach (var parameter in tempParams)
                        {
                            constructor.Parameters.Add(parameter.Type, parameter);
                        }

                        curClass.Constructors.Add(constructor.Name, constructor);
                        break;
                }
            }

            allClasses.Add(curClass.Name, curClass);
        }

        return allClasses;
    }

    private string CreateName(string name, List<Variable> parameters)
    {
        var res = new StringBuilder();
        res.Append(name).Append('(');
        foreach (var parameter in parameters)
        {
            res.Append(parameter.Type);
            res.Append(',');
        }

        res.Remove(res.Length - 1, 1);

        res.Append(')');

        return res.ToString();
    }

    private void AnalyzeClass(ClassDeclaration declaration)
    {
        var currentClass = classes[declaration.Name.ClassIdentifier.Name];
        foreach (var (key, method) in currentClass.Methods)
        {
            AnalyzeBody(method.MethodBody, currentClass, method.ReturnType);
        }

        foreach (var (key, constructor) in currentClass.Constructors)
        {
            AnalyzeBody(constructor.ConstructorBody, currentClass, "Void");
        }
    }

    private void AnalyzeBody(Body body, CurrentClass currentClass, string returnType)
    {
        var newVariables = new List<Variable>();
        var newTypes = new List<Variable>();

        foreach (var node in body.StatementsOrDeclarations)
        {
            switch (node)
            {
                case VariableDeclaration variableDeclaration:
                    newVariables.Add(new Variable
                    {
                        Name = variableDeclaration.VariableIdentifier.Name,
                        Type = EvalExpression(variableDeclaration.VariableExpression, currentClass)
                    });
                    break;
            }
        }
    }

    private string EvalExpression(Expression expression, CurrentClass currentClass)
    {
        // expression.PrimaryOrConstructorCall.
        var currentType = "";
        currentType = expression.PrimaryOrConstructorCall switch
        {
            Primary primary => primary.Node switch
            {
                IntegerLiteral => "Integer",
                RealLiteral => "Real",
                BooleanLiteral => "Boolean",
                This => currentClass.Name,
                ClassName className => className.ClassIdentifier.Name,
                _ => currentType
            },
            ConstructorCall constructorCall => constructorCall.ConstructorClassName.ClassIdentifier.Name,
            _ => currentType
        };

        foreach (var (identifier, arguments) in expression.Calls)
        {
            var curClass = classes[currentType];
            string name;
            if (arguments == null)
            {
                name = CreateName(identifier.Name, new List<Variable>());
            }
            else
            {
                var list = arguments.Expressions
                    .Select(expr => new Variable { Name = "", Type = EvalExpression(expr, currentClass) }).ToList();

                name = CreateName(identifier.Name, list);
            }

            if (!curClass.Methods.ContainsKey(name))
            {
                ReportNonFatal($"There is no such method: {name}");
            }

            var method = curClass.Methods[name];
        }

        return currentType;
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

    private void ReportNonFatal(string text)
    {
        // TODO(qexik): implement reporting
    }
}

class CurrentClass : IEquatable<CurrentClass>
{
    public string Name { get; init; }
    public string? Generic { get; init; }
    public string? BaseClass { get; init; }

    public Dictionary<string, Method> Methods { get; } = new();

    public Dictionary<string, Variable> Variables { get; } = new();

    public Dictionary<string, Constructor> Constructors { get; } = new();
    // public List<Variable> Variables { get; } = new();

    // TODO: rewrite Equals
    public bool Equals(CurrentClass? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return false;
        return Name.Equals(other.Name) &&
               ((BaseClass == null && other.BaseClass == null) || BaseClass!.Equals(other.BaseClass));
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
    public Body MethodBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();

    // TODO: fix
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
    public string Name { get; init; }
    public string Type { get; init; }
    public Body ConstructorBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();

    // TODO: fix
    public bool Equals(Constructor? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Type.Equals(other.Type) && Parameters.SequenceEqual(other.Parameters);
    }
}


// foreach (var memberDeclaration in declaration.Members)
//         {
//             var member = memberDeclaration.Member;
//             switch (member)
//             {
//                 case MethodDeclaration method:
//                     var methodParameters = new List<Variable>();
//                     if (method.MethodParameters != null)
//                     {
//                         methodParameters.AddRange(method.MethodParameters.ParameterDeclarations.Select(parameter =>
//                             new Variable
//                             {
//                                 Name = parameter.ParameterIdentifier.Name,
//                                 Type = GetTypeFromClassName(parameter.ParameterClassName)
//                             }));
//                     }
//
//                     AnalyzeBody(method.MethodBody, variables.Concat(methodParameters).ToList(), methods, currentClass,
//                         method.ReturnTypeIdentifier == null
//                             ? "Void"
//                             : GetTypeFromClassName(method.ReturnTypeIdentifier));
//                     break;
//                 case ConstructorDeclaration constructor:
//                     var constructorParameters = new List<Variable>();
//                     if (constructor.ConstructorParameters != null)
//                     {
//                         constructorParameters.AddRange(constructor.ConstructorParameters.ParameterDeclarations.Select(
//                             parameter =>
//                                 new Variable
//                                 {
//                                     Name = parameter.ParameterIdentifier.Name,
//                                     Type = GetTypeFromClassName(parameter.ParameterClassName)
//                                 }
//                         ));
//                     }
//
//                     AnalyzeBody(constructor.ConstructorBody, variables.Concat(constructorParameters).ToList(), methods,
//                         currentClass, "Void");
//                     break;
//             }
//         }

// private (List<Variable>, List<Method>, List<Constructor>) GetMembersWithTypes(ClassDeclaration declaration)
// {
//     var variables = new List<Variable>();
//     var methods = new List<Method>();
//     var constructors = new List<Constructor>();
//
//     foreach (var member in declaration.Members)
//     {
//         switch (member.Member)
//         {
//             case VariableDeclaration variableDeclaration:
//                 var varName = variableDeclaration.VariableIdentifier.Name;
//                 var className =
//                     (ClassName)((Primary)variableDeclaration.VariableExpression.PrimaryOrConstructorCall).Node;
//                 var type = GetTypeFromClassName(className);
//                 var variable = new Variable { Name = varName, Type = type };
//                 variables.Add(variable);
//                 break;
//             case MethodDeclaration methodDeclaration:
//                 var methodName = methodDeclaration.MethodIdentifier.Name;
//                 var returnType = methodDeclaration.ReturnTypeIdentifier;
//                 var methodType = returnType == null ? "Void" : GetTypeFromClassName(returnType);
//
//                 var method = new Method { Name = methodName, ReturnType = methodType };
//                 if (methodDeclaration.MethodParameters != null)
//                 {
//                     method.Parameters.AddRange(methodDeclaration.MethodParameters.ParameterDeclarations.Select(
//                         parameter =>
//                             GetTypeFromClassName(parameter.ParameterClassName)));
//                 }
//
//                 methods.Add(method);
//                 break;
//             case ConstructorDeclaration constructorDeclaration:
//                 var constructor = new Constructor { Type = GetTypeFromClassName(declaration.Name) };
//                 if (constructorDeclaration.ConstructorParameters != null)
//                 {
//                     constructor.Parameters.AddRange(
//                         constructorDeclaration.ConstructorParameters.ParameterDeclarations.Select(parameter =>
//                             GetTypeFromClassName(parameter.ParameterClassName)));
//                 }
//
//                 constructors.Add(constructor);
//                 break;
//         }
//     }
//
//     return (variables, methods, constructors);
// }