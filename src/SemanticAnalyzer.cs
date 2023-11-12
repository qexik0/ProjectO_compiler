using System.Text;
using OCompiler.nodes;
using Table = System.Collections.Generic.Dictionary<string, string>;

namespace OCompiler;

public class SemanticAnalyzer
{
    private Program _root;
    private List<Token> _tokens;
    private Dictionary<string, CurrentClass> _classes;
    private StreamWriter _report;

    public SemanticAnalyzer(Program root, List<Token> tokens, StreamWriter report)
    {
        _root = root;
        _tokens = tokens;
        _classes = CollectClasses(_root);
        _report = report;
        AddBasicClasses();
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
                            tmpParameters.AddRange(methodDeclaration.MethodParameters.ParameterDeclarations.Select(
                                parameter => new Variable
                                {
                                    Name = parameter.ParameterIdentifier.Name,
                                    Type = GetTypeFromClassName(parameter.ParameterClassName)
                                }));
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
                            method.Parameters.Add(parameter.Name, parameter);
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
                            tempParams.AddRange(
                                constructorDeclaration.ConstructorParameters.ParameterDeclarations.Select(parameter =>
                                    new Variable
                                    {
                                        Name = parameter.ParameterIdentifier.Name,
                                        Type = GetTypeFromClassName(parameter.ParameterClassName)
                                    }));
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
                            constructor.Parameters.Add(parameter.Name, parameter);
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
        var currentClass = _classes[declaration.Name.ClassIdentifier.Name];
        foreach (var (key, method) in currentClass.Methods)
        {
            AnalyzeBody(method.MethodBody!, currentClass, method.Name, method.Parameters, method.ReturnType,
                method.ReturnType != "Void");
        }

        foreach (var (key, constructor) in currentClass.Constructors)
        {
            AnalyzeBody(constructor.ConstructorBody!, currentClass, constructor.Name, constructor.Parameters, "Void",
                false);
        }
    }

    private bool AnalyzeBody(Body body, CurrentClass currentClass, string curMethod,
        Dictionary<string, Variable> outerVariables,
        string returnType, bool shouldReturn)
    {
        var newVariables = new Dictionary<string, Variable>();
        var hasReturn = false;

        foreach (var node in body.StatementsOrDeclarations)
        {
            var newDict = MergeDicts(newVariables, outerVariables);
            switch (node)
            {
                case VariableDeclaration variableDeclaration:
                    newVariables.Add(variableDeclaration.VariableIdentifier.Name, new Variable
                    {
                        Name = variableDeclaration.VariableIdentifier.Name,
                        Type = EvalExpression(variableDeclaration.VariableExpression, currentClass, newDict)
                    });
                    break;
                case Statement statement:
                    switch (statement.StatementNode)
                    {
                        case Assignment assignment:
                            if (!newVariables.ContainsKey(assignment.AssignmentIdentifier.Name) &&
                                !outerVariables.ContainsKey(assignment.AssignmentIdentifier.Name) &&
                                !currentClass.Variables.ContainsKey(assignment.AssignmentIdentifier.Name))
                            {
                                ReportFatal($"Undeclared Variable: {assignment.AssignmentIdentifier.Name}");
                                throw new Exception();
                            }

                            var varType =
                                newVariables.TryGetValue(assignment.AssignmentIdentifier.Name, out var variable)
                                    ? variable.Type
                                    : outerVariables.TryGetValue(assignment.AssignmentIdentifier.Name,
                                        out var outVariable)
                                        ? outVariable.Type
                                        : currentClass.Variables[assignment.AssignmentIdentifier.Name].Type;
                            var exprType = EvalExpression(assignment.AssignmentExpression, currentClass, newDict);
                            if (!CheckIfAncestor(exprType, varType))
                            {
                                ReportNonFatal(
                                    $"Cannot assign type {exprType} to Variable {assignment.AssignmentIdentifier.Name} of type {varType}");
                            }

                            break;
                        case WhileLoop whileLoop:
                            var conditionType =
                                EvalExpression(whileLoop.WhileConditionExpression, currentClass, newDict);
                            if (conditionType != "Boolean")
                            {
                                ReportNonFatal(
                                    $"Condition in WhileLoop should have type Boolean, but got {conditionType}");
                            }

                            hasReturn = AnalyzeBody(whileLoop.WhileBody, currentClass, curMethod, newDict, returnType,
                                false);
                            break;
                        case IfStatement ifStatement:
                            var ifConditionType =
                                EvalExpression(ifStatement.IfConditionExpression, currentClass, newDict);
                            if (ifConditionType != "Boolean")
                            {
                                ReportNonFatal(
                                    $"Condition in WhileLoop should have type Boolean, but got {ifConditionType}");
                            }

                            var ret1 = AnalyzeBody(ifStatement.IfBody, currentClass, curMethod, newDict, returnType,
                                false);
                            var ret2 = AnalyzeBody(ifStatement.ElseBody, currentClass, curMethod, newDict, returnType,
                                false);
                            hasReturn = ret1 && ret2;
                            break;
                        case ReturnStatement returnStatement:
                            hasReturn = true;
                            var type = returnStatement.ReturnExpression == null
                                ? "Void"
                                : EvalExpression(returnStatement.ReturnExpression, currentClass, newDict);
                            if (type != returnType)
                            {
                                ReportNonFatal(
                                    $"The method should return type {returnType}, but tries to return {type}");
                            }

                            break;
                    }

                    break;
            }
        }

        if (shouldReturn && !hasReturn)
        {
            ReportNonFatal($"Missing return statement in {curMethod}");
        }

        return hasReturn;
    }

    private Dictionary<string, Variable> MergeDicts(Dictionary<string, Variable> d1, Dictionary<string, Variable> d2)
    {
        var dict = new Dictionary<string, Variable>();
        foreach (var (key, value) in d1)
        {
            dict.Add(key, value);
        }

        foreach (var (key, value) in d2)
        {
            dict.Add(key, value);
        }

        return dict;
    }

    private string EvalExpression(Expression expression, CurrentClass currentClass,
        Dictionary<string, Variable> curVariables)
    {
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

        if (curVariables.TryGetValue(currentType, out var variable))
        {
            currentType = variable.Type;
        }
        else if (currentClass.Variables.TryGetValue(currentType, out var classVariable))
        {
            currentType = classVariable.Type;
        }

        foreach (var (identifier, arguments) in expression.Calls)
        {
            if (!_classes.ContainsKey(currentType))
            {
                ReportFatal($"There is no such Type {currentType}");
                throw new Exception();
            }

            var curClass = _classes[currentType];
            string name;
            if (arguments == null)
            {
                name = CreateName(identifier.Name, new List<Variable>());
            }
            else
            {
                var list = arguments.Expressions
                    .Select(expr => new Variable { Name = "", Type = EvalExpression(expr, currentClass, curVariables) })
                    .ToList();

                name = CreateName(identifier.Name, list);
            }

            if (!curClass.Methods.ContainsKey(name))
            {
                ReportFatal($"There is no such method: {name}");
                throw new Exception();
            }

            var method = curClass.Methods[name];
            currentType = method.ReturnType;
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

    private bool CheckIfAncestor(string className, string candidate)
    {
        if (className == candidate)
        {
            return true;
        }

        var curClass = _classes.TryGetValue(className, out var cur) ? cur.BaseClass : null;
        while (curClass != null)
        {
            if (curClass == candidate)
            {
                return true;
            }

            curClass = _classes.TryGetValue(curClass, out cur) ? cur.BaseClass : null;
        }

        return false;
    }

    private void ReportFatal(string text)
    {
        _report.WriteLine($"FATAL: {text}");
    }

    private void ReportNonFatal(string text)
    {
        _report.WriteLine($"NON-FATAL: {text}");
    }

    private void AddBasicClasses()
    {
        AddClass();
        AddAnyValue();
        AddAnyRef();
        AddInteger();
        AddReal();
        AddBoolean();
        AddArray();
        AddList();
    }

    private void AddClass()
    {
        var cl = new CurrentClass { Name = "Class" };

        _classes.Add("Class", cl);
    }

    private void AddAnyValue()
    {
        var anyValue = new CurrentClass { Name = "AnyValue", BaseClass = "Class" };

        _classes.Add("AnyValue", anyValue);
    }

    private void AddAnyRef()
    {
        var anyRef = new CurrentClass { Name = "AnyRef", BaseClass = "Class" };

        _classes.Add("AnyRef", anyRef);
    }

    private void AddInteger()
    {
        // create Integer
        var integer = new CurrentClass { Name = "Integer", BaseClass = "AnyValue" };
        // adding methods
        integer.Methods.Add("toReal()", new Method { Name = "toReal()", ReturnType = "Real" });
        integer.Methods.Add("toBoolean()", new Method { Name = "toBoolean()", ReturnType = "Boolean" });

        var unaryMinusMethod = new Method { Name = "UnaryMinus()", ReturnType = "Integer" };
        integer.Methods.Add("UnaryMinus()", unaryMinusMethod);

        var unaryPlusMethod = new Method { Name = "UnaryPlus()", ReturnType = "Integer" };
        integer.Methods.Add("UnaryPlus()", unaryPlusMethod);

        // Binary operations with Integer
        var plusMethod = new Method { Name = "Plus(Integer)", ReturnType = "Integer" };
        plusMethod.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Plus(Integer)", plusMethod);

        var minusMethodInt = new Method { Name = "Minus(Integer)", ReturnType = "Integer" };
        minusMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Minus(Integer)", minusMethodInt);

        var multMethodInt = new Method { Name = "Mult(Integer)", ReturnType = "Integer" };
        multMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Mult(Integer)", multMethodInt);

        var divMethodInt = new Method { Name = "Div(Integer)", ReturnType = "Integer" };
        divMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Div(Integer)", divMethodInt);

        var remMethod = new Method { Name = "Rem(Integer)", ReturnType = "Integer" };
        remMethod.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Rem(Integer)", remMethod);

        // Comparisons with Integer
        var lessMethodInt = new Method { Name = "Less(Integer)", ReturnType = "Boolean" };
        lessMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Less(Integer)", lessMethodInt);

        var lessEqualMethodInt = new Method { Name = "LessEqual(Integer)", ReturnType = "Boolean" };
        lessEqualMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("LessEqual(Integer)", lessEqualMethodInt);

        var greaterMethodInt = new Method { Name = "Greater(Integer)", ReturnType = "Boolean" };
        greaterMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Greater(Integer)", greaterMethodInt);

        var greaterEqualMethodInt = new Method { Name = "GreaterEqual(Integer)", ReturnType = "Boolean" };
        greaterEqualMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("GreaterEqual(Integer)", greaterEqualMethodInt);

        var equalMethodInt = new Method { Name = "Equal(Integer)", ReturnType = "Boolean" };
        equalMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Methods.Add("Equal(Integer)", equalMethodInt);

        // Binary operations with Real
        var plusMethodReal = new Method { Name = "Plus(Real)", ReturnType = "Real" };
        plusMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Plus(Real)", plusMethodReal);

        var minusMethodReal = new Method { Name = "Minus(Real)", ReturnType = "Real" };
        minusMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Minus(Real)", minusMethodReal);

        var multMethodReal = new Method { Name = "Mult(Real)", ReturnType = "Real" };
        multMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Mult(Real)", multMethodReal);

        var divMethodReal = new Method { Name = "Div(Real)", ReturnType = "Real" };
        divMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Div(Real)", divMethodReal);

        // Comparisons with Real
        var lessMethodReal = new Method { Name = "Less(Real)", ReturnType = "Boolean" };
        lessMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Less(Real)", lessMethodReal);

        var lessEqualMethodReal = new Method { Name = "LessEqual(Real)", ReturnType = "Boolean" };
        lessEqualMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("LessEqual(Real)", lessEqualMethodReal);

        var greaterMethodReal = new Method { Name = "Greater(Real)", ReturnType = "Boolean" };
        greaterMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Greater(Real)", greaterMethodReal);

        var greaterEqualMethodReal = new Method { Name = "GreaterEqual(Real)", ReturnType = "Boolean" };
        greaterEqualMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("GreaterEqual(Real)", greaterEqualMethodReal);

        var equalMethodReal = new Method { Name = "Equal(Real)", ReturnType = "Boolean" };
        equalMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Methods.Add("Equal(Real)", equalMethodReal);

        // adding constructors
        integer.Constructors.Add("Integer(Integer)", new Constructor { Name = "Integer(Integer)", Type = "Integer" });
        integer.Constructors.Add("Integer(Real)", new Constructor { Name = "Integer(Real)", Type = "Integer" });
        // adding variables
        integer.Variables.Add("Min", new Variable { Name = "Min", Type = "Integer" });
        integer.Variables.Add("Max", new Variable { Name = "Max", Type = "Integer" });

        _classes.Add("Integer", integer);
    }

    private void AddReal()
    {
        // create Real class
        var real = new CurrentClass { Name = "Real", BaseClass = "AnyValue" };

        // adding methods to Real
        real.Methods.Add("toInteger()", new Method { Name = "toInteger()", ReturnType = "Integer" });
        real.Methods.Add("toBoolean()", new Method { Name = "toBoolean()", ReturnType = "Boolean" });

        var unaryMinusRealMethod = new Method { Name = "UnaryMinus()", ReturnType = "Real" };
        real.Methods.Add("UnaryMinus()", unaryMinusRealMethod);

        var unaryPlusRealMethod = new Method { Name = "UnaryPlus()", ReturnType = "Real" };
        real.Methods.Add("UnaryPlus()", unaryPlusRealMethod);

        // Operations with Integer
        var plusMethodInt = new Method { Name = "Plus(Integer)", ReturnType = "Integer" };
        plusMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Plus(Integer)", plusMethodInt);

        var minusMethodInt = new Method { Name = "Minus(Integer)", ReturnType = "Integer" };
        minusMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Minus(Integer)", minusMethodInt);

        var multMethodInt = new Method { Name = "Mult(Integer)", ReturnType = "Integer" };
        multMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Mult(Integer)", multMethodInt);

        var divMethodInt = new Method { Name = "Div(Integer)", ReturnType = "Integer" };
        divMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Div(Integer)", divMethodInt);

        // Operations with Real
        var plusMethodReal = new Method { Name = "Plus(Real)", ReturnType = "Real" };
        plusMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Plus(Real)", plusMethodReal);

        var minusMethodReal = new Method { Name = "Minus(Real)", ReturnType = "Real" };
        minusMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Minus(Real)", minusMethodReal);

        var multMethodReal = new Method { Name = "Mult(Real)", ReturnType = "Real" };
        multMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Mult(Real)", multMethodReal);

        var divMethodReal = new Method { Name = "Div(Real)", ReturnType = "Real" };
        divMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Div(Real)", divMethodReal);

        var remMethod = new Method { Name = "Rem(Real)", ReturnType = "Real" };
        remMethod.Parameters.Add("p",
            new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Rem(Real)", remMethod);

        // Comparisons with Integer
        var lessMethodInt = new Method { Name = "Less(Integer)", ReturnType = "Boolean" };
        lessMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Less(Integer)", lessMethodInt);

        var lessEqualMethodInt = new Method { Name = "LessEqual(Integer)", ReturnType = "Boolean" };
        lessEqualMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("LessEqual(Integer)", lessEqualMethodInt);

        var greaterMethodInt = new Method { Name = "Greater(Integer)", ReturnType = "Boolean" };
        greaterMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Greater(Integer)", greaterMethodInt);

        var greaterEqualMethodInt = new Method { Name = "GreaterEqual(Integer)", ReturnType = "Boolean" };
        greaterEqualMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("GreaterEqual(Integer)", greaterEqualMethodInt);

        var equalMethodInt = new Method { Name = "Equal(Integer)", ReturnType = "Boolean" };
        equalMethodInt.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Methods.Add("Equal(Integer)", equalMethodInt);

        // Comparisons with Real
        var lessMethodReal = new Method { Name = "Less(Real)", ReturnType = "Boolean" };
        lessMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Less(Real)", lessMethodReal);

        var lessEqualMethodReal = new Method { Name = "LessEqual(Real)", ReturnType = "Boolean" };
        lessEqualMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("LessEqual(Real)", lessEqualMethodReal);

        var greaterMethodReal = new Method { Name = "Greater(Real)", ReturnType = "Boolean" };
        greaterMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Greater(Real)", greaterMethodReal);

        var greaterEqualMethodReal = new Method { Name = "GreaterEqual(Real)", ReturnType = "Boolean" };
        greaterEqualMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("GreaterEqual(Real)", greaterEqualMethodReal);

        var equalMethodReal = new Method { Name = "Equal(Real)", ReturnType = "Boolean" };
        equalMethodReal.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Methods.Add("Equal(Real)", equalMethodReal);

        // adding constructors to Real
        real.Constructors.Add("Real()", new Constructor { Name = "Real()" });
        real.Constructors.Add("Real(Integer)", new Constructor { Name = "Real(Integer)", Type = "Real" });
        real.Constructors.Add("Real(Real)", new Constructor { Name = "Real(Real)", Type = "Real" });

        // adding variables to Real
        real.Variables.Add("Min", new Variable { Name = "Min", Type = "Real" });
        real.Variables.Add("Max", new Variable { Name = "Max", Type = "Real" });
        real.Variables.Add("Epsilon", new Variable { Name = "Epsilon", Type = "Real" });

        _classes.Add("Real", real);
    }

    private void AddBoolean()
    {
        // create Boolean class
        var boolean = new CurrentClass { Name = "Boolean", BaseClass = "AnyValue" };

        // adding methods to Boolean
        boolean.Methods.Add("toInteger()", new Method { Name = "toInteger()", ReturnType = "Integer" });

        var orMethod = new Method { Name = "Or(Boolean)", ReturnType = "Boolean" };
        orMethod.Parameters.Add("p", new Variable { Name = "p", Type = "Boolean" });
        boolean.Methods.Add("Or(Boolean)", orMethod);

        var andMethod = new Method { Name = "And(Boolean)", ReturnType = "Boolean" };
        andMethod.Parameters.Add("p", new Variable { Name = "p", Type = "Boolean" });
        boolean.Methods.Add("And(Boolean)", andMethod);

        var xorMethod = new Method { Name = "Xor(Boolean)", ReturnType = "Boolean" };
        xorMethod.Parameters.Add("p", new Variable { Name = "p", Type = "Boolean" });
        boolean.Methods.Add("Xor(Boolean)", xorMethod);

        var notMethod = new Method { Name = "Not()", ReturnType = "Boolean" };
        boolean.Methods.Add("Not()", notMethod);

        // adding constructors to Boolean
        boolean.Constructors.Add("Boolean()", new Constructor { Name = "Boolean()" });
        boolean.Constructors.Add("Boolean(Boolean)", new Constructor { Name = "Boolean(Boolean)", Type = "Boolean" });

        _classes.Add("Boolean", boolean);
    }

    private void AddArray()
    {
        // create Array class
        var array = new CurrentClass { Name = "Array", Generic = "T", BaseClass = "AnyRef" };

        // adding methods to Array
        array.Methods.Add("toList()", new Method { Name = "toList()", ReturnType = "List" });
        array.Methods.Add("Length()", new Method { Name = "Length()", ReturnType = "Integer" });

        var getMethod = new Method { Name = "Get(Integer)", ReturnType = "T" };
        getMethod.Parameters.Add("i", new Variable { Name = "i", Type = "Integer" });
        array.Methods.Add("Get(Integer)", getMethod);

        var setMethod = new Method { Name = "Set(Integer,T)", ReturnType = "Void" };
        setMethod.Parameters.Add("i", new Variable { Name = "i", Type = "Integer" });
        setMethod.Parameters.Add("v", new Variable { Name = "v", Type = "T" });
        array.Methods.Add("Set(Integer,T)", setMethod);

        // adding constructors to Array
        array.Constructors.Add("Array()", new Constructor { Name = "Array()" });
        array.Constructors.Add("Array(Integer)", new Constructor { Name = "Array(Integer)", Type = "Array" });

        _classes.Add("Array", array);
    }

    private void AddList()
    {
        // create List class
        var list = new CurrentClass { Name = "List", Generic = "T", BaseClass = "AnyRef" };

        // adding methods to List
        var appendMethod = new Method { Name = "Append(T)", ReturnType = "List" };
        appendMethod.Parameters.Add("v", new Variable { Name = "v", Type = "T" });
        list.Methods.Add("Append(T)", appendMethod);

        var headMethod = new Method { Name = "Head()", ReturnType = "T" };
        list.Methods.Add("Head()", headMethod);

        var tailMethod = new Method { Name = "Tail()", ReturnType = "T" };
        list.Methods.Add("Tail()", tailMethod);

        // adding constructors to List
        list.Constructors.Add("List()", new Constructor { Name = "List()" });
        // The empty parameter constructor is the same as the default, so it may not need to be added again.
        list.Constructors.Add("List(T)", new Constructor { Name = "List(T)", Type = "List" });
        var constructorWithCount = new Constructor { Name = "List(T,Integer)", Type = "List" };
        constructorWithCount.Parameters.Add("p", new Variable { Name = "p", Type = "T" });
        constructorWithCount.Parameters.Add("count", new Variable { Name = "count", Type = "Integer" });
        list.Constructors.Add("List(T,Integer`)", constructorWithCount);

        _classes.Add("List", list);
    }
}

class CurrentClass
{
    public string Name { get; init; }
    public string? Generic { get; init; }
    public string? BaseClass { get; init; }

    public Dictionary<string, Method> Methods { get; } = new();

    public Dictionary<string, Variable> Variables { get; } = new();

    public Dictionary<string, Constructor> Constructors { get; } = new();
}

class Variable
{
    public string Name { get; init; }
    public string Type { get; init; }
}

class Method
{
    public string Name { get; init; }
    public string ReturnType { get; init; }
    public Body? MethodBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();
}

class Constructor
{
    public string Name { get; init; }
    public string Type { get; init; }
    public Body? ConstructorBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();
}