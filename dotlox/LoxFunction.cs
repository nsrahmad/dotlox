namespace dotlox;

public class LoxFunction(Stmt.Function declaration) : ILoxCallable
{
    private Stmt.Function _declaration = declaration;

    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new Environment(interpreter.Globals);
        for (int i = 0; i < _declaration.Params.Count; i++)
        {
            environment.Define(_declaration.Params[i].Lexeme, arguments[i]);
        }

        interpreter.ExecuteBlock(_declaration.body, environment);
        return null;
    }

    public int Arity()
    {
        return _declaration.Params.Count;
    }


    public override string ToString()
    {
        return $"<Fn {_declaration.Name.Lexeme}>";
    }
}