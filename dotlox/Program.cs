using static System.Console;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

namespace dotlox;

internal static class Program
{
    static bool hadError = false;

    static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            WriteLine("Usage: dotlox [script]");
            System.Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }
    // Error handling
    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
        hadError = true;
    }

    static void RunPrompt()
    {
        while (true)
        {
            Write("> ");
            var line = ReadLine();
            if (line == null) break;
            Run(line);
            hadError = false;
        }
    }

    static void RunFile(string v)
    {
        var path = Combine(GetCurrentDirectory(), v);
        Run(ReadAllText(path));
        if (hadError) Environment.Exit(65);
    }

    static void Run(string v)
    {
        var scanner = new Scanner(v);
        List<Token> tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            WriteLine(token);
        }
    }
}