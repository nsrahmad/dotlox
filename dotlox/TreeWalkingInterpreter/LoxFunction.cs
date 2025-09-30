namespace dotlox.TreeWalkingInterpreter;

public class LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer) : ILoxCallable
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
            return isInitializer ? closure.GetAt(0, "this") : returnValue.Value;
        }

        return isInitializer ? closure.GetAt(0, "this") : null;
    }

    public int Arity()
    {
        return declaration.Params.Count;
    }


    public override string ToString()
    {
        return $"<Fn {declaration.Name.Lexeme}>";
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var env = new Environment(closure);
        env.Define("this", instance);
        return new LoxFunction(declaration, env, isInitializer);
    }
}