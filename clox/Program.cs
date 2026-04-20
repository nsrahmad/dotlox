
using clox;

if (args.Length == 0)
{
    Repl();
}
else if(args.Length == 1)
{
    RunFile(args[0]);
}
else
{
    Console.Error.WriteLine("Usage: clox [path]");
    Environment.Exit(64);
}

return 0;

void Repl()
{
    var vm = new VM();
    while (true)
    {
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line is null)
        {
            break;
        }

        vm.Interpret(line.AsSpan());
    }
}

void RunFile(string path)
{
    var vm = new VM();
    try
    {
        vm.Interpret(File.ReadAllText(path).AsSpan());
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}
