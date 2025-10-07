using CommunityToolkit.HighPerformance.Buffers;

namespace clox;

public struct Chunk() : IDisposable
{
    private int _count = 0;
    private int _constCount = 0;
    private MemoryOwner<int> _lines = MemoryOwner<int>.Allocate(8);

    public MemoryOwner<byte> Code = MemoryOwner<byte>.Allocate(8);
    public MemoryOwner<double> Constants = MemoryOwner<double>.Allocate(8);

    public void WriteChunk(byte code, int line)
    {
        if (Code.Length < _count + 1)
        {
            // Grow the Memory if needed
            var newCode  = MemoryOwner<byte>.Allocate(Code.Length * 2);
            Code.Span.CopyTo(newCode.Span);
            Code.Dispose();
            Code = newCode;

            // Same for lines buffer
            var newLines = MemoryOwner<int>.Allocate(Code.Length * 2);
            _lines.Span.CopyTo(newLines.Span);
            _lines.Dispose();
            _lines = newLines;
        }

        Code.Span[_count] = code;
        _lines.Span[_count] = line;
        _count++;
    }

    public int AddConstant(double constant)
    {
        // If constants is full, increase capacity
        if (Constants.Length < _constCount + 1)
        {
            var newConst = MemoryOwner<double>.Allocate(Constants.Length * 2);
            Constants.Span.CopyTo(newConst.Span);
            Constants.Dispose();
            Constants = newConst;
        }

        Constants.Span[_constCount] = constant;
        _constCount++;
        return _constCount - 1; // returns the index of current constant

    }

    /// <summary>
    /// Pretty prints the disassembled byte code showing instructions and their parameters (if any).
    /// </summary>
    /// <param name="name">The name of the current chunk of the byte code</param>
    public void Disassemble(string name)
    {
        Console.WriteLine($"== {name} == ");
        for(var offset = 0; offset < _count;)
        {
            offset = DisassembleInstruction(offset);
        }
    }

    public int DisassembleInstruction(int offset)
    {
        Console.Write($"{offset:D4} ");

        if (offset > 0 && _lines.Span[offset] == _lines.Span[offset - 1])
        {
            Console.Write("   | ");
        }
        else
        {
            Console.Write($"{_lines.Span[offset],4} ");
        }

        switch ((OpCode)Code.Span[offset])
        {
            case OpCode.RETURN:
                return SimpleInstruction(nameof(OpCode.RETURN), offset);
            case OpCode.CONSTANT:
                return ConstantInstruction(nameof(OpCode.CONSTANT), offset);
            case OpCode.ADD:
                return SimpleInstruction(nameof(OpCode.ADD), offset);
            case OpCode.SUBSTRACT:
                return SimpleInstruction(nameof(OpCode.SUBSTRACT), offset);
            case OpCode.MULTIPLY:
                return SimpleInstruction(nameof(OpCode.MULTIPLY), offset);
            case OpCode.DIVIDE:
                return SimpleInstruction(nameof(OpCode.DIVIDE), offset);
            case OpCode.NEGATE:
                return SimpleInstruction(nameof(OpCode.NEGATE), offset);
            default:
                Console.WriteLine($"Unknown opcode {Code.Span[offset]}");
                return offset + 1;
        }
    }

    private int SimpleInstruction(string name, int offset)
    {
        Console.WriteLine(name);
        return offset + 1;
    }

    private int ConstantInstruction(string name, int offset)
    {
        var constant = Code.Span[offset + 1];
        Console.Write(format: "{0, -16} {1:D4} ", arg0: name, arg1: constant);
        PrintValue(Constants.Span[constant]);
        Console.WriteLine();
        return offset + 2;
    }

    private void PrintValue(double d)
    {
        Console.Write(d);
    }

    public void Dispose()
    {
        Code.Dispose();
        _lines.Dispose();
        Constants.Dispose();
    }
}

