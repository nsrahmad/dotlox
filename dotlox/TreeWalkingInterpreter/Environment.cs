using System.Diagnostics;

namespace dotlox.TreeWalkingInterpreter;

public class Environment
{
    private readonly Environment? _enclosing;

    public Environment(Environment enclosing)
    {
        _enclosing = enclosing;
    }

    public Environment()
    {
        _enclosing = null;
    }

    private readonly Dictionary<string, object> _values = new();

    public void Define(string name, object? value)
    {
        _values.Add(name, value!);
    }

    public object Get(Token name)
    {
        return _values.TryGetValue(name.Lexeme, out var value)
            ? value
            : _enclosing != null
                ? _enclosing.Get(name)
                : throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }
        if (_enclosing != null)
        {
            _enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object GetAt(int distance, string name)
    {
        return Ancestor(distance)._values[name];
    }

    private Environment Ancestor(int distance)
    {
        var environment = this;
        for (var i = 0; i < distance; i++)
        {
            if (environment != null) environment = environment._enclosing;
        }

        Debug.Assert(environment != null, nameof(environment) + " != null");
        return environment;
    }

    public void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance)._values[name.Lexeme] = value;
    }
}