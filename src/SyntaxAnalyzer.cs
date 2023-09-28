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

    /// <summary>
    /// Method that starts to parse program
    /// </summary>
    /// <returns>Program root node</returns>
    public Program ParseProgram()
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
        var className = new ClassName() { ClassIdentifier = identifier };
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
                    $"Syntax error: Expected constructor, variable declaration, or method but found {nextToken.Type}")
            };
            list.Add(new MemberDeclaration() {Member = res});

            nextToken = PeekToken();
        }

        return list;
    }

    private VariableDeclaration ParseVariableDeclaration()
    {
        throw new Exception();
    }

    private MethodDeclaration ParseMethodDeclaration()
    {
        throw new Exception();
    }

    private ConstructorDeclaration ParseConstructorDeclaration()
    {
        throw new Exception();
    }

    /// <summary>
    /// Method that checks if token with given type can be consumed and consume it
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
    /// Method that consumes next token with given type if possible, otherwise throws exception
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
            throw new Exception($"Syntax error: Expected {type} but found end of file");
        }

        throw new Exception($"Syntax error: Expected {type} but found {_tokens[_index].Type}");
    }

    /// <summary>
    /// Method to peek the next token not consuming it
    /// </summary>
    /// <returns>The next token</returns>
    /// <exception cref="Exception">Throws and exception if there is no next token</exception>
    private Token PeekToken()
    {
        if (_index < _tokens.Count)
        {
            return _tokens[_index];
        }

        throw new Exception("Syntax error: Expected token but found end of file");
    }
}