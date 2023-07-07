using static System.Console;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

namespace dotlox;

internal static class Program
{
    private static readonly Interpreter Interpreter = new Interpreter();
    private static bool _hadError;
    private static bool _hadRuntimeError;

    private static void Main(string[] args)
    {
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
    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, " at '" + token.Lexeme + "'", message);
        }
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
        if (_hadError) System.Environment.Exit(65);
        if (_hadRuntimeError) System.Environment.Exit(70);
    }

    private static void Run(string v)
    {
        var scanner = new Scanner(v);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        var statements = parser.Parse();
        if (_hadError) return;
        Interpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.Error.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
        _hadRuntimeError = true;
    }
}