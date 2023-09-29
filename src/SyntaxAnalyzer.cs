using OCompiler.nodes;

namespace OCompiler;

public class SyntaxAnalyzer
{
    private List<Token> _tokens;
    private int _index;

    public SyntaxAnalyzer(List<Token> tokens)
    {
        _tokens = tokens;
        _index = 0;
    }

    public AstNode? RunAnalyzer()
    {
        try
        {
            return ParseProgram();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Syntax Analyzing failed with the following error:\n{e.Message}");
        }

        return null;
    }

    /// <summary>
    /// Method that starts to parse program
    /// </summary>
    /// <returns>Program root node</returns>
    private Program ParseProgram()
    {
        // Create root node
        var program = new Program();

        // Til end of file try to parse classes
        while (_index < _tokens.Count)
        {
            var res = ParseClassDeclaration();
            program.ProgramClasses.Add(res);
        }

        return program;
    }

    /// <summary>
    /// Method that parses class declaration
    /// </summary>
    /// <returns>Node with information about class declaration</returns>
    private ClassDeclaration ParseClassDeclaration()
    {
        // consume `class` keyword with class name
        ConsumeToken(TokenType.Class);
        var className = ParseClassName();

        // if we are able to consume `extends` keyword, then parse class name
        ClassName? baseClassName = null;
        if (MaybeConsumeToken(TokenType.Extends))
        {
            baseClassName = ParseClassName();
        }

        // then consume `is`, members of class, and `end`
        ConsumeToken(TokenType.Is);
        var memberDeclarations = ParseMemberDeclarations();
        ConsumeToken(TokenType.End);

        var classDeclaration = new ClassDeclaration() { Name = className, BaseClassName = baseClassName };
        classDeclaration.Members.AddRange(memberDeclarations);

        return classDeclaration;
    }

    /// <summary>
    /// Method that parses class name
    /// </summary>
    /// <returns>ClassName node</returns>
    private ClassName ParseClassName()
    {
        var identifier = ParseIdentifier();
        ClassName? genericClassname = null;

        if (MaybeConsumeToken(TokenType.LeftSquareBracket))
        {
            genericClassname = ParseClassName();
            ConsumeToken(TokenType.RightSquareBracket);
        }

        var className = new ClassName() { ClassIdentifier = identifier, GenericClassName = genericClassname };
        return className;
    }

    /// <summary>
    /// Method that parses identifier
    /// </summary>
    /// <returns>Identifier node</returns>
    private Identifier ParseIdentifier()
    {
        var value = ConsumeToken(TokenType.Identifier);
        var identifier = new Identifier() { Name = value.Value };
        return identifier;
    }

    /// <summary>
    /// Method that parses method declarations from inside of class
    /// </summary>
    /// <returns> List of MemberDeclaration nodes</returns>
    private IEnumerable<MemberDeclaration> ParseMemberDeclarations()
    {
        var nextToken = PeekToken();
        var list = new List<MemberDeclaration>();

        while (nextToken.Type != TokenType.End)
        {
            AstNode res = nextToken.Type switch
            {
                // parse variable declaration
                TokenType.Var => ParseVariableDeclaration(),
                // parse method
                TokenType.Method => ParseMethodDeclaration(),
                // parse constructor
                TokenType.This => ParseConstructorDeclaration(),
                _ => throw new Exception(
                    $"Syntax error at {nextToken.LineNumber}:{nextToken.ColumnNumber} : Expected constructor, variable declaration, or method but found {nextToken.Type}")
            };
            list.Add(new MemberDeclaration() { Member = res });

            nextToken = PeekToken();
        }

        return list;
    }

    /// <summary>
    /// Method that parses variable declaration
    /// </summary>
    /// <returns>VariableDeclaration node</returns>
    private VariableDeclaration ParseVariableDeclaration()
    {
        ConsumeToken(TokenType.Var);
        var identifier = ParseIdentifier();
        ConsumeToken(TokenType.Colon);
        var expression = ParseExpression();

        var variableDeclaration = new VariableDeclaration()
            { VariableIdentifier = identifier, VariableExpression = expression };

        return variableDeclaration;
    }

    /// <summary>
    /// Method that parses method declaration  
    /// </summary>
    /// <returns>MethodDeclaration node</returns>
    private MethodDeclaration ParseMethodDeclaration()
    {
        ConsumeToken(TokenType.Method);
        var identifier = ParseIdentifier();
        var parameters = ParseParameters();

        Identifier? returnType = null;
        if (MaybeConsumeToken(TokenType.Colon))
        {
            returnType = ParseIdentifier();
        }

        var body = ParseBody();
        ConsumeToken(TokenType.End);

        var methodDeclaration = new MethodDeclaration()
        {
            MethodBody = body, MethodIdentifier = identifier, MethodParameters = parameters,
            ReturnTypeIdentifier = returnType
        };

        return methodDeclaration;
    }

    /// <summary>
    /// Method that parses constructor declaration
    /// </summary>
    /// <returns>ConstructorDeclaration node</returns>
    private ConstructorDeclaration ParseConstructorDeclaration()
    {
        ConsumeToken(TokenType.This);
        var parameters = ParseParameters();
        ConsumeToken(TokenType.Is);
        var body = ParseBody();
        ConsumeToken(TokenType.End);

        var constructorDeclaration = new ConstructorDeclaration()
            { ConstructorBody = body, ConstructorParameters = parameters };

        return constructorDeclaration;
    }

    /// <summary>
    /// Method that parses parameters
    /// </summary>
    /// <returns>null if there are no parameters, Parameters otherwise</returns>
    private Parameters? ParseParameters()
    {
        ConsumeToken(TokenType.LeftParanthesis);
        if (MaybeConsumeToken(TokenType.RightParanthesis))
        {
            return null;
        }

        var parameters = new Parameters();
        while (PeekToken().Type != TokenType.RightParanthesis)
        {
            var parameterDeclaration = ParseParameterDeclaration();
            parameters.ParameterDeclarations.Add(parameterDeclaration);
        }

        return parameters;
    }

    /// <summary>
    /// Method that parses body
    /// </summary>
    /// <returns>Body with list of statements or declarations</returns>
    private Body ParseBody()
    {
        var body = new Body();

        while (PeekToken().Type != TokenType.End)
        {
            AstNode node = PeekToken().Type == TokenType.Var ? ParseVariableDeclaration() : ParseStatement();

            body.StatementsOrDeclarations.Add(node);
        }

        return body;
    }

    /// <summary>
    /// Method that parses parameter declaration
    /// </summary>
    /// <returns>ParameterDeclaration node with class name and identifier</returns>
    private ParameterDeclaration ParseParameterDeclaration()
    {
        var identifier = ParseIdentifier();
        ConsumeToken(TokenType.Colon);
        var className = ParseClassName();

        var parameterDeclaration = new ParameterDeclaration()
            { ParameterClassName = className, ParameterIdentifier = identifier };

        return parameterDeclaration;
    }

    /// <summary>
    /// Method that parses expression
    /// </summary>
    /// <returns>Expression node</returns>
    private Expression ParseExpression()
    {
        var primary = ParsePrimary();
        var expression = new Expression() { EntityPrimary = primary };

        while (MaybeConsumeToken(TokenType.Dot))
        {
            var identifier = ParseIdentifier();
            var arguments = ParseArguments();
            expression.Calls.Add((identifier, arguments));
        }

        return expression;
    }

    /// <summary>
    /// Method that parses Statement
    /// </summary>
    /// <returns>Statement node</returns>
    /// <exception cref="Exception">Throws an exception if there was no correct statement found</exception>
    private Statement ParseStatement()
    {
        var nextToken = PeekToken();
        AstNode node = nextToken.Type switch
        {
            TokenType.Identifier => ParseAssignment(),
            TokenType.While => ParseWhileLoop(),
            TokenType.If => ParseIfStatement(),
            TokenType.Return => ParseReturnStatement(),
            _ => throw new Exception(
                $"Syntax error at {nextToken.LineNumber}:{nextToken.ColumnNumber} : Expected assignment, while loop, is statement, or return statement, but found {nextToken.Type}")
        };

        var statement = new Statement() { StatementNode = node };
        return statement;
    }

    /// <summary>
    /// Method that parses assignment expression
    /// </summary>
    /// <returns>Assignment node</returns>
    private Assignment ParseAssignment()
    {
        var identifier = ParseIdentifier();
        ConsumeToken(TokenType.Assignment);
        var expression = ParseExpression();

        var assignment = new Assignment() { AssignmentIdentifier = identifier, AssignmentExpression = expression };

        return assignment;
    }

    /// <summary>
    /// Method that parses while loop
    /// </summary>
    /// <returns>WhileLoop node</returns>
    private WhileLoop ParseWhileLoop()
    {
        ConsumeToken(TokenType.While);
        var expression = ParseExpression();
        ConsumeToken(TokenType.Loop);
        var body = ParseBody();
        ConsumeToken(TokenType.End);

        var whileLoop = new WhileLoop() { WhileConditionExpression = expression, WileBody = body };

        return whileLoop;
    }

    /// <summary>
    /// Method that parses if statement
    /// </summary>
    /// <returns>IfStatement node</returns>
    private IfStatement ParseIfStatement()
    {
        ConsumeToken(TokenType.If);
        var expression = ParseExpression();
        ConsumeToken(TokenType.Then);
        var body = ParseBody();
        Body? elseBody = null;
        if (MaybeConsumeToken(TokenType.Else))
        {
            elseBody = ParseBody();
        }

        ConsumeToken(TokenType.End);

        var ifStatement = new IfStatement() { IfConditionExpression = expression, IfBody = body, ElseBody = elseBody };

        return ifStatement;
    }

    /// <summary>
    /// Method that parses return statement
    /// </summary>
    /// <returns>ReturnStatement node with Expression if there is one</returns>
    private ReturnStatement ParseReturnStatement()
    {
        ConsumeToken(TokenType.Return);
        Expression? expression = null;
        if (!CheckForStatement() && !CheckForVariableDeclaration() && PeekToken().Type != TokenType.End)
        {
            expression = ParseExpression();
        }

        if (CheckForStatement() || CheckForVariableDeclaration())
        {
            ReportWarning($"Unreachable code at {PeekToken().LineNumber}:{PeekToken().ColumnNumber}.");
        }

        var returnStatement = new ReturnStatement() { ReturnExpression = expression };
        return returnStatement;
    }

    /// <summary>
    /// Method that parses primary of expression
    /// </summary>
    /// <returns>Primary node</returns>
    /// <exception cref="Exception">Throws an exception if there was no correct primary found</exception>
    private Primary ParsePrimary()
    {
        var nextToken = PeekToken();
        AstNode node = nextToken.Type switch
        {
            TokenType.IntegerLiteral => ParseIntegerLiteral(),
            TokenType.RealLiteral => ParseRealLiteral(),
            TokenType.False or TokenType.True => ParseBooleanLiteral(),
            TokenType.This => ParseThis(),
            TokenType.Identifier => ParseClassName(),
            _ => throw new Exception(
                $"Syntax error at {nextToken.LineNumber}:{nextToken.ColumnNumber} : Expected literal, `this`, or identifier, but found {nextToken.Type}")
        };

        var primary = new Primary() { Node = node };

        return primary;
    }

    /// <summary>
    /// Method that parses arguments
    /// </summary>
    /// <returns>null is there are no arguments, Arguments otherwise</returns>
    private Arguments? ParseArguments()
    {
        ConsumeToken(TokenType.LeftParanthesis);
        if (MaybeConsumeToken(TokenType.RightParanthesis))
        {
            return null;
        }

        var arguments = new Arguments();

        while (!MaybeConsumeToken(TokenType.RightParanthesis))
        {
            var expression = ParseExpression();
            arguments.Expressions.Add(expression);
        }

        return arguments;
    }

    /// <summary>
    /// Method that parses integer literal
    /// </summary>
    /// <returns>IntegerLiteral node</returns>
    private IntegerLiteral ParseIntegerLiteral()
    {
        var token = ConsumeToken(TokenType.IntegerLiteral);
        var value = int.Parse(token.Value);
        var integerLiteral = new IntegerLiteral() { Value = value };
        return integerLiteral;
    }

    /// <summary>
    /// Method that parses real literal
    /// </summary>
    /// <returns>RealLiteral node</returns>
    private RealLiteral ParseRealLiteral()
    {
        var token = ConsumeToken(TokenType.RealLiteral);
        var value = double.Parse(token.Value);
        var realLiteral = new RealLiteral() { Value = value };
        return realLiteral;
    }

    /// <summary>
    /// Method that parses boolean literal
    /// </summary>
    /// <returns>BooleanLiteral node</returns>
    private BooleanLiteral ParseBooleanLiteral()
    {
        var token = PeekToken().Type == TokenType.False ? ConsumeToken(TokenType.False) : ConsumeToken(TokenType.True);
        var value = bool.Parse(token.Value);
        var booleanLiteral = new BooleanLiteral() { Value = value };
        return booleanLiteral;
    }

    /// <summary>
    /// Method that parses `this` keyword
    /// </summary>
    /// <returns>This node</returns>
    private This ParseThis()
    {
        ConsumeToken(TokenType.This);
        return new This();
    }

    /// <summary>
    /// Helper method to check if there is a statement in front of current token 
    /// </summary>
    /// <returns>bool value if there is a statement</returns>
    private bool CheckForStatement()
    {
        var res = PeekToken().Type switch
        {
            TokenType.Identifier => PeekToken(1).Type == TokenType.Assignment,
            TokenType.While => true,
            TokenType.If => true,
            TokenType.Return => true,
            _ => false
        };

        return res;
    }

    /// <summary>
    /// Helper method to check is there is a variable declaration in front of current token 
    /// </summary>
    /// <returns>bool value if there is a variable declaration</returns>
    private bool CheckForVariableDeclaration()
    {
        return PeekToken().Type == TokenType.Var;
    }

    /// <summary>
    /// Helper method that checks if token with given type can be consumed and consume it
    /// </summary>
    /// <param name="type">TokenType for consume</param>
    /// <returns>bool value if token was consumed or not</returns>
    private bool MaybeConsumeToken(TokenType type)
    {
        if (_index >= _tokens.Count || _tokens[_index].Type != type) return false;
        _index++;
        return true;
    }

    /// <summary>
    /// Helper method that consumes next token with given type if possible, otherwise throws exception
    /// </summary>
    /// <param name="type">TokenType for consume</param>
    /// <returns>Consumed Token</returns>
    /// <exception cref="Exception">Throws an exception if token has wrong type, ar there is no next token</exception>
    private Token ConsumeToken(TokenType type)
    {
        if (_index < _tokens.Count && _tokens[_index].Type == type)
        {
            _index++;
            return _tokens[_index - 1];
        }

        if (_index >= _tokens.Count)
        {
            throw new Exception(
                $"Syntax error at {_tokens.Last().LineNumber}:{_tokens.Last().ColumnNumber} : Expected {type} but found end of file");
        }

        throw new Exception(
            $"Syntax error at {_tokens[_index].LineNumber}:{_tokens[_index].ColumnNumber} : Expected {type} but found {_tokens[_index].Type}");
    }

    /// <summary>
    /// Helper method to peek the next token not consuming it
    /// </summary>
    /// <returns>The next token</returns>
    /// <exception cref="Exception">Throws and exception if there is no next token</exception>
    private Token PeekToken(int offset = 0)
    {
        if (_index < _tokens.Count)
        {
            return _tokens[_index + offset];
        }

        throw new Exception("Syntax error: Expected token but found end of file");
    }

    /// <summary>
    /// Helper method to report warnings
    /// </summary>
    /// <param name="message">Message to display as warning</param>
    private void ReportWarning(string message)
    {
        Console.WriteLine($"Warning: {message}");
    }
}