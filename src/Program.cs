using System.CommandLine;
using System.Globalization;
using System.Text;
using OCompiler.nodes;

namespace OCompiler;

public class CLIFrontent
{
    public static void Main(string[] args)
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        var rootCommand = new RootCommand("Project O language compiler");
        var inputFile = new Option<FileInfo>(name: "--input", description: "Input source file");
        var outputFile = new Option<string>(name: "--output", description: "Output path for the lexical report");
        var lexicalReport = new Command("lexer-report", "Produce a lexical analysis report")
        {
            inputFile,
            outputFile
        };
        var syntaxReport = new Command("syntax-report", "Produce a lexical analysis report")
        {
            inputFile,
            outputFile
        };
        var semanticReport = new Command("semantic-report", "Produce a semantic analysis report")
        {
            inputFile,
            outputFile
        };
        lexicalReport.SetHandler((input, output) =>
        {
            var lexer = new Lexer();
            using var source = new StreamReader(input.FullName);
            var tokens = lexer.Tokenize(source);
            using var report = new StreamWriter(output);
            report.WriteLine("Tokens:");
            foreach (var token in tokens)
            {
                report.WriteLine($"{token.Value} - {token.Type} at {token.LineNumber}:{token.ColumnNumber}");
            }
        }, inputFile, outputFile);
        syntaxReport.SetHandler((input, output) =>
        {
            var lexer = new Lexer();
            using var source = new StreamReader(input.FullName);
            using var report = new StreamWriter(output);
            var tokens = lexer.Tokenize(source);
            if (tokens.Any(x => x.Type == TokenType.Undefined))
            {
                var failedTokens = tokens.Where(x => x.Type == TokenType.Undefined).ToArray();
                report.WriteLine("Lexical analysis failed. Undefined tokens:");
                foreach (var token in failedTokens)
                {
                    report.WriteLine($"{token.Value} at {token.LineNumber}:{token.ColumnNumber}");
                }

                report.WriteLine("Syntax analysis aborted.");
                return;
            }

            var syntaxer = new SyntaxAnalyzer(tokens);
            nodes.AstNode program;
            try
            {
                program = syntaxer.RunAnalyzer();
            }
            catch (Exception ex)
            {
                report.WriteLine($"Syntax Analyzing failed with the following error:\n{ex.Message}");
                return;
            }

            report.WriteLine("Syntax analyzing finished successfully! The AST:");
            report.WriteLine(PrettifySExpression(program.ToString()));

            string PrettifySExpression(string input)
            {
                StringBuilder sb = new();
                int indent = 0;
                foreach (var c in input)
                {
                    if (c == '(')
                    {
                        ++indent;
                        sb.AppendLine();
                        sb.Append(' ', indent * 2);
                    }
                    else if (c == ')')
                    {
                        --indent;
                    }

                    sb.Append(c);
                }

                sb.Remove(0, Environment.NewLine.Length);
                return sb.ToString();
            }
        }, inputFile, outputFile);
        semanticReport.SetHandler((input, output) =>
        {
            var lexer = new Lexer();
            using var source = new StreamReader(input.FullName);
            using var report = new StreamWriter(output);
            var tokens = lexer.Tokenize(source);
            if (tokens.Any(x => x.Type == TokenType.Undefined))
            {
                var failedTokens = tokens.Where(x => x.Type == TokenType.Undefined).ToArray();
                report.WriteLine("Lexical analysis failed. Undefined tokens:");
                foreach (var token in failedTokens)
                {
                    report.WriteLine($"{token.Value} at {token.LineNumber}:{token.ColumnNumber}");
                }

                report.WriteLine("Syntax analysis aborted.");
                return;
            }

            var syntaxer = new SyntaxAnalyzer(tokens);
            nodes.AstNode program;
            try
            {
                program = syntaxer.RunAnalyzer();
            }
            catch (Exception ex)
            {
                report.WriteLine($"Syntax Analyzing failed with the following error:\n{ex.Message}");
                return;
            }
            
            ((Program) program).CodeGen();

            report.WriteLine("Syntax analyzing finished successfully!\n");
            report.WriteLine("Semantics Report:");
            try
            {
                var semanticAnalyzer = new SemanticAnalyzer((nodes.Program)program, tokens, report);
                semanticAnalyzer.AnalyzeProgram();
                report.WriteLine("Semantic Analyzing finished successfully!");
            }
            catch (Exception)
            {
                report.WriteLine("Semantic Analyzing finished with errors!");
            }
        }, inputFile, outputFile);
        rootCommand.Add(lexicalReport);
        rootCommand.Add(syntaxReport);
        rootCommand.Add(semanticReport);
        rootCommand.Invoke(args);
    }
}