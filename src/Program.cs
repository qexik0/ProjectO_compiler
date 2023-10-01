using System.CommandLine;
using System.Globalization;

namespace OCompiler;

public class CLIFrontent
{
    public static void Main(string[] args)
    {
        CultureInfo ci = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        var rootCommand = new RootCommand("Project O language compiler");
        var inputFile = new Option<FileInfo>(name: "--input",description: "Input source file");
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
        lexicalReport.SetHandler((input, output) => {
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
            report.WriteLine("Syntax analyzing finished successfully!");
            /*PrintAST(program);

            void PrintAST(nodes.AstNode cur, int depth = 0)
            {
                
            }*/
        }, inputFile, outputFile);
        rootCommand.Add(lexicalReport);
        rootCommand.Add(syntaxReport);
        rootCommand.Invoke(args);
    }
}