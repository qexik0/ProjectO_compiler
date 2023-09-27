using System.Text.RegularExpressions;

namespace OCompiler;

public class Lexer
{
    private readonly Regex _tokenizer;
    private readonly Dictionary<string, TokenType> _keywords;
    public Lexer()
    {
        _tokenizer = new Regex(@"(-?\d+(\.\d+)?([eE]-?\d+)?(?![a-zA-Z])|[_a-zA-Z][_a-zA-Z0-9]*|:=|\(|\)|,|:|\[|\]|\.|//|/\*|\*/|\n|[^\s]+)");
        _keywords = new();
        _keywords["class"] = TokenType.Class;
        _keywords["extends"] = TokenType.Extends;
        _keywords["is"] = TokenType.Is;
        _keywords["end"] = TokenType.End;
        _keywords["var"] = TokenType.Var;
        _keywords[":"] = TokenType.Colon;
        _keywords["method"] = TokenType.Method;
        _keywords["("] = TokenType.LeftParanthesis;
        _keywords[")"] = TokenType.RightParanthesis;
        _keywords[","] = TokenType.Comma;
        _keywords["this"] = TokenType.This;
        _keywords[":="] = TokenType.Assignment;
        _keywords["while"] = TokenType.While;
        _keywords["loop"] = TokenType.Loop;
        _keywords["if"] = TokenType.If;
        _keywords["then"] = TokenType.Then;
        _keywords["else"] = TokenType.Else;
        _keywords["["] = TokenType.LeftSquareBracket;
        _keywords["]"] = TokenType.RightSquareBracket;
        _keywords["return"] = TokenType.Return;
        _keywords["."] = TokenType.Dot;
        _keywords["true"] = TokenType.True;
        _keywords["false"] = TokenType.False;
    }

    public List<Token> Tokenize(StreamReader input)
    {
        List<Token> res = new();
        string source = input.ReadToEnd();
        int ln = 1, index = 0;
        bool lineComment = false;
        bool blockComment = false;
        foreach (Match match in _tokenizer.Matches(source))
        {
            int cn = match.Index - index + 1;

            if (match.Value == "\n")
            {
                ln++;
                index = match.Index + 1;
                lineComment = false;
                continue;
            }

            if (blockComment && match.Value == "*/")
            {
                blockComment = false;
                continue;
            }
            else if (match.Value == "*/")
            {
                res.Add(new Token() {Type = TokenType.Undefined, Value = match.Value, LineNumber = ln, ColumnNumber = cn});
                continue;
            }
            else if (match.Value == "/*")
            {
                blockComment = true;
                continue;
            }

            if (match.Value == "//")
            {
                lineComment = true;
            }
            
            if (lineComment || blockComment)
            {
                continue;
            }

            if (_keywords.ContainsKey(match.Value))
            {
                res.Add(new Token() {Type = _keywords[match.Value], Value = match.Value, LineNumber = ln, ColumnNumber = cn});
            }
            else if (int.TryParse(match.Value, out int _))
            {
                res.Add(new Token() {Type = TokenType.IntegerLiteral, Value = match.Value, LineNumber = ln, ColumnNumber = cn});
            }
            else if (double.TryParse(match.Value, out double _))
            {
                res.Add(new Token() {Type = TokenType.RealLiteral, Value = match.Value, LineNumber = ln, ColumnNumber = cn});
            }
            else if (char.IsLetter(match.Value[0]) && match.Value.All(x => char.IsAsciiLetterOrDigit(x) || x == '_'))
            {  
                res.Add(new Token() {Type = TokenType.Identifier, Value = match.Value, LineNumber = ln, ColumnNumber = cn});
            }
            else
            {
                res.Add(new Token() {Type = TokenType.Undefined, Value = match.Value, LineNumber = ln, ColumnNumber = cn});
            }
        }
        return res;
    }
}