namespace dotlox;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    private readonly Environment _globals = new();
    private Environment _environment;
    private readonly Dictionary<Expr, int> _locals = new();

    public Interpreter()
    {
        _environment = _globals;
        _globals.Define("clock", new FunctionalCallable(0, (_, _) => (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds));
    }

    // A concrete implementation that wraps delegates.
    private class FunctionalCallable(int arity, Func<Interpreter, List<object>, object> call) : ILoxCallable
    {
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            return call(interpreter, arguments);
        }

        public int Arity()
        {
            return arity;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    private static string Stringify(object? obj)
    {
        if (obj == null)
        {
            return "nil";
        }
        var text = obj.ToString()!;
        if (text.EndsWith(".0"))
        {
            text = text[..^2];
        }
        return text;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            case TokenType.GREATER:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left <= (double)right;
            case TokenType.MINUS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                return left switch
                {
                    double l when right is double r => l + r,
                    string sl when right is string sr => sl + sr,
                    _ => throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.")
                };
            case TokenType.SLASH:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperand(expr.Operator, left, right);
                return (double)left * (double)right;
        }

        return null!;
    }

    public object VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        var arguments = expr.Arguments.Select(Evaluate).ToList();

        if (callee is not ILoxCallable function)
            throw new RuntimeError(expr.Paren, "Can only call functions and classes");

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expr.Paren, "Expected " +
                                               function.Arity() + " arguments but got " +
                                               arguments.Count + ".");
        }

        return function.Call(this, arguments) ?? 0;
    }

    private static void CheckNumberOperand(Token op, object left, object right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(op, "Operands must be numbers.");
    }

    private static bool IsEqual(object? left, object? right)
    {
        return left switch
        {
            null when right == null => true,
            null => false,
            _ => left.Equals(right)
        };
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    private object Evaluate(Expr expression)
    {
        return expression.Accept(this);
    }

    public object VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        return expr.Operator.Type switch
        {
            TokenType.BANG => !IsTruthy(right),
            TokenType.MINUS => -(double)right,
            _ => null!
        };
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return LookupVariable(expr.Name, expr);
    }

    private object LookupVariable(Token name, Expr expr)
    {
        return _locals.TryGetValue(expr, out var distance) ? _environment.GetAt(distance, name.Lexeme) : _globals.Get(name);
    }

    private static bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        var b = obj as bool?;
        return b ?? true;

    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt, _environment);
        _environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.thenBranch);
        }
        else if (stmt.elseBranch != null)
        {
            Execute(stmt.elseBranch);
        }
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    // ReSharper disable once ReturnTypeCanBeNotNullable
    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value != null) value = Evaluate(stmt.Value);
        throw new Return(value);
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.condition)))
        {
            Execute(stmt.body);
        }

        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        _environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(_environment));
        return null;
    }

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        var previous = _environment;
        try
        {
            _environment = environment;
            foreach (var item in statements)
            {
                Execute(item);
            }
        }
        finally
        {
            _environment = previous;
        }
    }

    public object VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);

        if (_locals.TryGetValue(expr, out var distance))
        {
            _environment.AssignAt(distance, expr.Name, value);
        }
        else
        {
            _globals.Assign(expr.Name, value);
        }

        return value;
    }

    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }
}

public class Return(object? value) : Exception
{
    public readonly object? Value = value;
}

public interface ILoxCallable
{
    object? Call(Interpreter interpreter, List<object> arguments);
    int Arity();
}

public class RuntimeError(Token token, string message) : Exception(message)
{
    public Token Token { get; } = token;
}
