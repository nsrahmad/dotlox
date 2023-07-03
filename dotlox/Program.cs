using static System.Console;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

namespace dotlox;

internal static class Program
{
    private static bool _hadError = false;

    private static void Main(string[] args)
    {
        // // Ast Printer
        // var expression = new Expr.Binary(
        //     new Expr.Unary(
        //         new Token(TokenType.MINUS, "-", null, 1),
        //         new Expr.Literal(123)),
        //     new Token(TokenType.STAR, "*", null, 1),
        //     new Expr.Grouping(
        //         new Expr.Literal(45.67)));
        //
        // Console.WriteLine(new AstPrinter().Print(expression));
        
        switch (args.Length)
        {
            case > 1:
                WriteLine("Usage: dotlox [script]");
                System.Environment.Exit(64);
                break;
            case 1:
                RunFile(args[0]);
                break;
            default:
                RunPrompt();
                break;
        }
    }
    // Error handling
    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        _hadError = true;
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Write("> ");
            var line = ReadLine();
            if (line == null) break;
            Run(line);
            _hadError = false;
        }
    }

    private static void RunFile(string v)
    {
        var path = Combine(GetCurrentDirectory(), v);
        Run(ReadAllText(path));
        if (_hadError) Environment.Exit(65);
    }

    private static void Run(string v)
    {
        var scanner = new Scanner(v);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            WriteLine(token);
        }
    }
}