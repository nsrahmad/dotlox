namespace clox;

public struct Compiler
{
    public static void Compile(ReadOnlySpan<char> source)
    {
        var scanner = new Scanner();
        scanner.InitScanner(source);
        var line = -1;

        while (true)
        {
            var token = scanner.ScanToken();
            if (token.Line != line)
            {
                Console.Write($"{token.Line,4} ");
                line = token.Line;
            }
            else
            {
                Console.Write("   | ");
            }
            Console.WriteLine($"{token.Type} '{scanner.Source[token.Start..(token.Start + token.Length)]}'");
            if (token.Type == TokenType.EOF)
            {
                break;
            }
        }
    }
}