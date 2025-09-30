namespace dotlox.TreeWalkingInterpreter;

public class LoxClass(string name, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public string Name => name;
    private Dictionary<string, LoxFunction> Methods => methods;

    public override string ToString()
    {
        return name;
    }

    public object Call(Interpreter interpreter, List<object> arguments)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        initializer?.Bind(instance).Call(interpreter, arguments);

        return instance;
    }

    public int Arity()
    {
        return  FindMethod("init")?.Arity() ?? 0;
    }

    public LoxFunction? FindMethod(string method)
    {
        return Methods.GetValueOrDefault(method);
    }
}