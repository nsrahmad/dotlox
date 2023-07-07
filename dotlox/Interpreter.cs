namespace dotlox;

public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object?>
{
    private readonly Environment _environment = new();
    
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
            Program.RuntimeError(e);
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
                return (double) left > (double) right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left >= (double) right;
            case TokenType.LESS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left < (double) right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left <= (double) right;
            case TokenType.MINUS:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left - (double) right;
            case TokenType.PLUS:
                switch (left)
                {
                    case double l when right is double r:
                        return l + r;
                    case string sl when right is string sr:
                        return sl + sr;
                }
                throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            case TokenType.SLASH:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left / (double) right;
            case TokenType.STAR:
                CheckNumberOperand(expr.Operator, left, right);
                return (double) left * (double) right;
        }

        return null!;
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

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        switch (expr.Operator.Type)
        {
            case TokenType.BANG:
                return !IsTruthy(right);
            case TokenType.MINUS:
                return -(double) right;
        }

        return null!;
    }

    public object VisitVariableExpr(Expr.Variable expr)
    {
        return _environment.Get(expr.Name);
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

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
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
}

public class RuntimeError : Exception
{
    public Token Token { get; init; }
    
    public RuntimeError(Token token, string message) : base(message)
    { 
        Token = token;
    }
}