namespace dotlox.TreeWalkingInterpreter;

public interface ILoxCallable
{
    object? Call(TreeWalkingInterpreter.Interpreter interpreter, List<object> arguments);
    int Arity();
}