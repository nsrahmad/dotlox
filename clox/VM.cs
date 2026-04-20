using System.Runtime.CompilerServices;

namespace clox;

public enum InterpretResult
{
    OK,
    COMPILE_ERROR,
    RUNTIME_ERROR,
}
internal ref struct VM()
{
    private byte _ip;
    private Chunk _chunk;
    private Stack<double> _stack = new(256);

     public InterpretResult Interpret(ReadOnlySpan<char> source)
     {
         Compiler.Compile(source);
         return InterpretResult.OK;
     }

    private InterpretResult Run()
    {
        while (true)
        {
#if DEBUG
            // show the current stack contents
            Console.Write("[");
            foreach (var item in _stack)
            {
                Console.Write($" {item}");   
            }
            Console.WriteLine(" ]");
            _chunk.DisassembleInstruction(_ip);
#endif
            switch ((OpCode)_chunk.Code.Span[_ip++])
            {
                // width -> 1 byte
                case OpCode.RETURN:
                    Console.WriteLine(_stack.Pop());
                    return InterpretResult.OK;
                // width -> 1 byte
                case OpCode.ADD:
                    _stack.Push(_stack.Pop() + _stack.Pop());
                    break;
                case OpCode.SUBSTRACT:
                    _stack.Push(_stack.Pop() - _stack.Pop());
                    break;
                case OpCode.MULTIPLY:
                    _stack.Push(_stack.Pop() * _stack.Pop());
                    break;
                case OpCode.DIVIDE:
                    _stack.Push(_stack.Pop() / _stack.Pop());
                    break;
                // width -> 1 byte
                case OpCode.NEGATE:
                    _stack.Push(-_stack.Pop());
                    break;

                // width -> 2 bytes ; first is instruction itself, next is the index of constant in the constants array
                case OpCode.CONSTANT:
                    _stack.Push(_chunk.Constants.Span[_chunk.Code.Span[_ip++]]);
                    break;
                default: 
                    return InterpretResult.RUNTIME_ERROR;
            }
        }
    }
}
