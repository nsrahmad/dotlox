namespace dotlox;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private enum FunctionType
    {
        NONE,
        FUNCTION
    }

    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.NONE;

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var argument in expr.Arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (_scopes.Count != 0 && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool val))
        {
            if (val is false) Lox.Error(expr.Name, "Can't read local variables in its own initializer.");
        }

        ResolveLocal(expr, expr.Name);
        return null;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = 0; i < _scopes.Count; i++)
        {
            if (_scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    private void BeginScope()
    {
        _scopes.Push(new Dictionary<string, bool>());
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = _currentFunction;
        _currentFunction = type;
        BeginScope();
        foreach (var param in function.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.body);
        EndScope();
        _currentFunction = enclosingFunction;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (_currentFunction == FunctionType.NONE)
        {
            Lox.Error(stmt.Keyword, "Can't return from top level code.");
        }
        if (stmt.Value != null)
        {
            Resolve(stmt.Value);
        }

        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.body);
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }

        Define(stmt.Name);
        return null;
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void Define(Token name)
    {
        if (_scopes.Count == 0) return;
        _scopes.Peek()[name.Lexeme] = true;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count == 0) return;
        var scope = _scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }

        scope.Add(name.Lexeme, false);
    }
}