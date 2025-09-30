namespace dotlox.TreeWalkingInterpreter;

public class LoxInstance(LoxClass klass)
{
    private readonly Dictionary<string, object> _fields = new();
    public LoxClass Klass => klass;

    public override string ToString()
    {
        return $"{klass.Name} instance";
    }

    public object Get(Token exprName)
    {
        if (_fields.TryGetValue(exprName.Lexeme, out var field))
            return field;

        var method = klass.FindMethod(exprName.Lexeme);
        if (method is not null)
        {
            return method.Bind(this);
        }

        throw new RuntimeError(exprName, $"Undefined property '{exprName.Lexeme}'.");
    }

    public void Set(Token name, object val)
    {
        _fields[name.Lexeme] = val;
    }
}