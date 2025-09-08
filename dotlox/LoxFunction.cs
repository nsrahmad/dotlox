namespace dotlox;

public class LoxFunction(Stmt.Function declaration, Environment closure) : ILoxCallable
{
    public object? Call(Interpreter interpreter, List<object> arguments)
    {
        var environment = new Environment(closure);
        for (var i = 0; i < declaration.Params.Count; i++)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.body, environment);
        }
        catch (Return returnValue)
        {
            return returnValue.Value;
        }
        return null;
    }

    public int Arity()
    {
        return declaration.Params.Count;
    }


    public override string ToString()
    {
        return $"<Fn {declaration.Name.Lexeme}>";
    }
}