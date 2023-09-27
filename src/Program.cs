﻿using System.CommandLine;
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
        var outputFile = new Option<string>(name: "--output", description: "Output path for the lecical report");
        var lexicalReport = new Command("lexer-report", "Produce a lexical analysis report")
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
        rootCommand.Add(lexicalReport);
        rootCommand.Invoke(args);
    }
}