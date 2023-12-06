using System.Text;
using OCompiler.nodes;
using Table = System.Collections.Generic.Dictionary<string, string>;

namespace OCompiler;

public class SemanticAnalyzer
{
    private readonly Program _root;
    private Dictionary<string, CurrentClass> _classes;
    private readonly StreamWriter _report;
    private bool _hasErrors;

    public SemanticAnalyzer(Program root, StreamWriter report)
    {
        _root = root;
        _report = report;
        _classes = CollectClasses(_root);
        AddBasicClasses();
    }

    /// <summary>
    /// Analyzes program
    /// </summary>
    /// <returns>true when analyzing successful, false otherwise</returns>
    public bool AnalyzeProgram()
    {
        try
        {
            foreach (var declaration in _root.ProgramClasses)
            {
                AnalyzeVariables(declaration);
            }

            foreach (var declaration in _root.ProgramClasses)
            {
                AnalyzeClass(declaration);
            }
        }
        catch (AnalyzerException)
        {
            _hasErrors = true;
        }

        _report.WriteLine(_hasErrors ? "Semantic Analyzer found errors!" : "Semantic Analyzing finished successfully!");

        return !_hasErrors;
    }

    private void AnalyzeVariables(ClassDeclaration declaration)
    {
        var currentClass = _classes[declaration.Name.ClassIdentifier.Name];
        foreach (var member in declaration.Members)
        {
            if (member.Member is VariableDeclaration variableDeclaration)
            {
                currentClass.Variables[variableDeclaration.VariableIdentifier.Name].Type = EvalExpression(
                    variableDeclaration.VariableExpression, currentClass, new Dictionary<string, Variable>());
            }
        }
    }

    private Dictionary<string, CurrentClass> CollectClasses(Program root)
    {
        Dictionary<string, CurrentClass> allClasses = new();
        foreach (var classDeclaration in root.ProgramClasses)
        {
            var curClass = new CurrentClass
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

                        if (!curClass.Methods.TryAdd(method.Name, method))
                        {
                            ReportFatal($"The method {method.Name} is already defined in class {curClass.Name}!");
                            throw new AnalyzerException();
                        }

                        break;
                    case VariableDeclaration variableDeclaration:
                        var variable = new Variable
                        {
                            Name = variableDeclaration.VariableIdentifier.Name,
                            Type = "Unknown"
                        };
                        if (!curClass.Variables.TryAdd(variable.Name, variable))
                        {
                            ReportFatal($"The variable {variable.Name} is already defined in class {curClass.Name}!");
                            throw new AnalyzerException();
                        }

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

                        if (!curClass.Constructors.TryAdd(constructor.Name, constructor))
                        {
                            ReportFatal(
                                $"The constructor {constructor.Name} is already defined in class {curClass.Name}!");
                            throw new AnalyzerException();
                        }

                        break;
                }
            }

            if (allClasses.TryAdd(curClass.Name, curClass)) continue;
            ReportFatal($"The class {curClass.Name} is already defined!");
            throw new AnalyzerException();
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

        if (parameters.Count > 0) res.Remove(res.Length - 1, 1);

        res.Append(')');

        return res.ToString();
    }

    private void AnalyzeClass(ClassDeclaration declaration)
    {
        var currentClass = _classes[declaration.Name.ClassIdentifier.Name];
        AnalyzeInheritance(currentClass);

        foreach (var (_, method) in currentClass.Methods)
        {
            AnalyzeParameters(method.Parameters);
            AnalyzeBody(method.MethodBody!, currentClass, method.Name, method.Parameters, method.ReturnType,
                method.ReturnType != "Void");
        }

        foreach (var (_, constructor) in currentClass.Constructors)
        {
            AnalyzeParameters(constructor.Parameters);
            AnalyzeBody(constructor.ConstructorBody!, currentClass, constructor.Name, constructor.Parameters, "Void",
                false);
        }
    }

    private void AnalyzeInheritance(CurrentClass cl)
    {
        var currentClass = cl;
        var set = new HashSet<string> { currentClass.Name };
        while (currentClass is { BaseClass: not null })
        {
            if (set.Contains(currentClass.BaseClass))
            {
                ReportFatal($"The error occured while analyzing inheritance for class {cl.Name}");
                throw new AnalyzerException();
            }

            set.Add(currentClass.BaseClass);
            if (_classes.TryGetValue(currentClass.BaseClass, out var res))
            {
                currentClass = res;
            }
            else
            {
                ReportNonFatal($"Type {currentClass.BaseClass} is not defined!");
                currentClass = null;
            }
        }
    }

    private void AnalyzeParameters(Dictionary<string, Variable> parameters)
    {
        foreach (var (_, parameter) in parameters)
        {
            if (!_classes.ContainsKey(parameter.Type))
            {
                ReportNonFatal($"Type {parameter.Type} is not defined!");
            }
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
            if (hasReturn)
            {
                ReportInfo(
                    $"Unreachable code after return in {(curMethod[0] == '(' ? "Constructor " + curMethod : curMethod)} of {currentClass.Name}");
            }

            switch (node)
            {
                case VariableDeclaration variableDeclaration:
                    var varExprType = EvalExpression(variableDeclaration.VariableExpression, currentClass, newDict);
                    var generic = varExprType.Contains(',') ? varExprType.Split(',')[1] : null;
                    varExprType = varExprType.Split(',')[0];
                    if (varExprType == "Void")
                    {
                        ReportFatal(
                            $"Unable to assign type Void to variable {variableDeclaration.VariableIdentifier.Name}");
                    }
                    else if (!_classes.ContainsKey(varExprType))
                    {
                        ReportNonFatal($"Type {varExprType} is not defined!");
                    }

                    if (!newVariables.TryAdd(variableDeclaration.VariableIdentifier.Name, new Variable
                        {
                            Name = variableDeclaration.VariableIdentifier.Name,
                            Type = generic != null ? $"{varExprType},{generic}" : varExprType
                        }))
                    {
                        ReportFatal(
                            $"The variable {variableDeclaration.VariableIdentifier.Name} is already defined in scope in method {(curMethod[0] == '(' ? "Constructor " + curMethod : curMethod)}!");
                        throw new AnalyzerException();
                    }

                    break;
                case Statement statement:
                    switch (statement.StatementNode)
                    {
                        case Assignment assignment:
                            if (!newVariables.ContainsKey(assignment.AssignmentIdentifier.Name) &&
                                !outerVariables.ContainsKey(assignment.AssignmentIdentifier.Name) &&
                                !currentClass.ContainsVariable(assignment.AssignmentIdentifier.Name, _classes))
                            {
                                ReportFatal($"Undeclared Variable: {assignment.AssignmentIdentifier.Name}");
                                throw new AnalyzerException();
                            }

                            var varType =
                                newVariables.TryGetValue(assignment.AssignmentIdentifier.Name, out var variable)
                                    ? variable.Type
                                    : outerVariables.TryGetValue(assignment.AssignmentIdentifier.Name,
                                        out var outVariable)
                                        ? outVariable.Type
                                        : currentClass.GetVariable(assignment.AssignmentIdentifier.Name, _classes)!
                                            .Type;
                            var exprType = EvalExpression(assignment.AssignmentExpression, currentClass, newDict);
                            if (!_classes.ContainsKey(exprType))
                            {
                                ReportNonFatal($"Type {exprType} is not defined!");
                            }

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
                            var ret2 = ifStatement.ElseBody != null && AnalyzeBody(ifStatement.ElseBody, currentClass,
                                curMethod, newDict, returnType, false);
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
                        case Expression expression:
                            EvalExpression(expression, currentClass, newDict);
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

        if (expression.PrimaryOrConstructorCall is ConstructorCall call)
        {
            var tempParams = (from exp in call.ConstructorArguments?.Expressions ?? new List<Expression>()
                select new Variable { Type = EvalExpression(exp, currentClass, curVariables) }).ToList();

            var name = CreateName("", tempParams);
            if (!_classes.ContainsKey(currentType))
            {
                ReportFatal($"Class {currentType} is not defined!");
                throw new AnalyzerException();
            }

            if (!_classes[currentType].ContainsConstructor(name, _classes))
            {
                ReportNonFatal($"There is no constructors matching {name} in class {currentType}!");
            }

            if (_classes[currentType].Generic != null && call.ConstructorClassName.GenericClassName != null)
            {
                currentType += $",{call.ConstructorClassName.GenericClassName.ClassIdentifier.Name}";
            }
        }

        foreach (var (identifier, arguments) in expression.Calls)
        {
            string? generic = null;
            if (currentType.Contains(','))
            {
                generic = currentType.Split(',')[1];
                currentType = currentType.Split(',')[0];
            }

            if (!_classes.ContainsKey(currentType))
            {
                ReportFatal($"There is no such Type {currentType}");
                throw new AnalyzerException();
            }

            var curClass = _classes[currentType];
            string name;
            if (arguments == null)
            {
                name = CreateName(identifier.Name, new List<Variable>());

                var curVariable = curClass.GetVariable(identifier.Name, _classes);
                if (curVariable != null)
                {
                    currentType = curVariable.Type;
                    continue;
                }
            }
            else
            {
                var list = arguments.Expressions
                    .Select(expr => new Variable { Name = "", Type = EvalExpression(expr, currentClass, curVariables) })
                    .ToList();

                name = CreateName(identifier.Name, list);
            }

            if (!curClass.ContainsMethod(name, _classes))
            {
                ReportFatal($"There is no such method or variable in class {curClass.Name}: {name}");
                throw new AnalyzerException();
            }

            var method = curClass.GetMethod(name, _classes);
            currentType = method!.ReturnType;
            if (currentType == curClass.Generic)
            {
                if (generic == null)
                {
                    ReportFatal("Something strange is going on!");
                    throw new AnalyzerException();
                }

                currentType = generic;
            }
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
        _hasErrors = true;
    }

    private void ReportInfo(string text)
    {
        _report.WriteLine($"INFO: {text}");
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
        AddConsole();
    }

    private void AddClass()
    {
        var cl = new CurrentClass { Name = "Class", BaseClass = "AnyRef" };

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
        var intConstructor = new Constructor { Name = "Integer(Integer)", Type = "Integer" };
        intConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        integer.Constructors.Add("(Integer)", intConstructor);

        var realConstructor = new Constructor { Name = "Integer(Real)", Type = "Integer" };
        realConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        integer.Constructors.Add("(Real)", realConstructor);

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
        real.Constructors.Add("()", new Constructor { Name = "Real()", Type = "Real" });

        var intConstructor = new Constructor { Name = "Real(Integer)", Type = "Real" };
        intConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        real.Constructors.Add("(Integer)", intConstructor);

        var realConstructor = new Constructor { Name = "Real(Real)", Type = "Real" };
        realConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        real.Constructors.Add("(Real)", realConstructor);
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

        boolean.Constructors.Add("()", new Constructor { Name = "Boolean()", Type = "Boolean" });

        var boolConstructor = new Constructor { Name = "Boolean(Boolean)", Type = "Boolean" };
        boolConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Boolean" });
        boolean.Constructors.Add("(Boolean)", boolConstructor);

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
        array.Constructors.Add("()", new Constructor { Name = "Array()", Type = "Array" });

        var intConstructor = new Constructor { Name = "Array(Integer)", Type = "Array" };
        intConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        array.Constructors.Add("(Integer)", intConstructor);

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
        list.Constructors.Add("()", new Constructor { Name = "()", Type = "List" });

        var tConstructor = new Constructor { Name = "(T)", Type = "List" };
        tConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "T" });
        list.Constructors.Add("(T)", tConstructor);

        var constructorWithCount = new Constructor { Name = "(T,Integer)", Type = "List" };
        constructorWithCount.Parameters.Add("p", new Variable { Name = "p", Type = "T" });
        constructorWithCount.Parameters.Add("count", new Variable { Name = "count", Type = "Integer" });
        list.Constructors.Add("(T,Integer)", constructorWithCount);

        _classes.Add("List", list);
    }

    private void AddConsole()
    {
        // create Console class
        var console = new CurrentClass { Name = "Console", BaseClass = "AnyRef" };

        // adding constructors to Console
        var intConstructor = new Constructor { Name = "(Integer)", Type = "Console" };
        intConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Integer" });
        console.Constructors.Add("(Integer)", intConstructor);

        var realConstructor = new Constructor { Name = "(Real)", Type = "Console" };
        realConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Real" });
        console.Constructors.Add("(Real)", realConstructor);

        var boolConstructor = new Constructor { Name = "(Boolean)", Type = "Console" };
        boolConstructor.Parameters.Add("p", new Variable { Name = "p", Type = "Boolean" });
        console.Constructors.Add("(Boolean)", boolConstructor);

        _classes.Add("Console", console);
    }
}

internal class AnalyzerException : Exception
{
}

internal class CurrentClass
{
    public string Name { get; init; } = "";
    public string? Generic { get; init; }
    public string? BaseClass { get; init; }

    public Dictionary<string, Method> Methods { get; } = new();

    public Dictionary<string, Variable> Variables { get; } = new();

    public Dictionary<string, Constructor> Constructors { get; } = new();

    public bool ContainsMethod(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Methods.ContainsKey(name)) return true;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Methods.ContainsKey(name)) return true;
            curClass = classes[curClass].BaseClass;
        }

        return false;
    }

    public Method? GetMethod(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Methods.TryGetValue(name, out var method)) return method;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Methods.TryGetValue(name, out var value)) return value;
            curClass = classes[curClass].BaseClass;
        }

        return null;
    }

    public bool ContainsVariable(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Variables.ContainsKey(name)) return true;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Variables.ContainsKey(name)) return true;
            curClass = classes[curClass].BaseClass;
        }

        return false;
    }

    public Variable? GetVariable(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Variables.TryGetValue(name, out var variable)) return variable;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Variables.TryGetValue(name, out Variable? value)) return value;
            curClass = classes[curClass].BaseClass;
        }

        return null;
    }

    public bool ContainsConstructor(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Constructors.ContainsKey(name)) return true;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Constructors.ContainsKey(name)) return true;
            curClass = classes[curClass].BaseClass;
        }

        return false;
    }

    public Constructor? GetConstructor(string name, Dictionary<string, CurrentClass> classes)
    {
        if (Constructors.TryGetValue(name, out var constructor)) return constructor;

        var curClass = BaseClass;
        while (curClass != null)
        {
            if (classes[curClass].Constructors.TryGetValue(name, out Constructor? value)) return value;
            curClass = classes[curClass].BaseClass;
        }

        return null;
    }
}

internal class Variable
{
    public string Name { get; init; } = "";
    public string Type { get; set; } = "";
}

class Method
{
    public string Name { get; init; } = "";
    public string ReturnType { get; init; } = "";
    public Body? MethodBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();
}

internal class Constructor
{
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public Body? ConstructorBody { get; init; }
    public Dictionary<string, Variable> Parameters { get; } = new();
}