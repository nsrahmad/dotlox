using static System.Console;
using static System.IO.Directory;
using static System.IO.File;
using static System.IO.Path;

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

void RunPrompt()
{
    while (true)
    {
        Write("> ");
        var line = ReadLine();
        if (line == null) break;
        Run(line);
    }
}

void RunFile(string v)
{
    var path = Combine(GetCurrentDirectory(), v);
    Run(ReadAllText(path));
}

void Run(string v)
{
    var scanner = new Scanner(v);
    List<Token> tokens = scanner.ScanTokens();

    foreach(var token in tokens)
    {
        WriteLine(token);
    }
}