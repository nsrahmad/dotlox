
using clox;

var vm = new VM();
var chunk = new Chunk();

chunk.WriteChunk((byte)OpCode.CONSTANT, 123);
chunk.WriteChunk((byte)chunk.AddConstant(1.2), 123);

chunk.WriteChunk((byte)OpCode.CONSTANT, 123);
chunk.WriteChunk((byte)chunk.AddConstant(1.2), 123);

chunk.WriteChunk((byte)OpCode.ADD, 123);

chunk.WriteChunk((byte)OpCode.CONSTANT, 123);
chunk.WriteChunk((byte)chunk.AddConstant(3.14), 123);

chunk.WriteChunk((byte)OpCode.MULTIPLY, 123);
chunk.WriteChunk((byte)OpCode.NEGATE, 123);

chunk.WriteChunk((byte)OpCode.RETURN, 123);

Console.WriteLine(vm.Interpret(ref chunk));

chunk.Dispose();
return 0;
