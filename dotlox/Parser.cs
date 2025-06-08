namespace dotlox;

using static TokenType;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            return Match(VAR) ? VarDeclaration() : Statement();
        }
        catch (ParseError e)
        {
            Console.Error.WriteLine(e.Message);
            Synchronize();
            return null!;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expect variable name.");
        Expr? initializer = null;
        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(FOR)) return ForStatement();
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(WHILE)) return WhileStatement();
        return Match(LEFT_BRACE) ? new Stmt.Block(Block()) : ExpressionStatement();
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer;
        if (Match(SEMICOLON))
        {
            initializer = null;
        } else if (Match(VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after loop condition.");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(RIGHT_PAREN, "Expect ')' after 'for' loop");

        Stmt body = Statement();

        // de-sugaring for loop into while loop
        if (increment != null)
        {
            body = new Stmt.Block([body, new Stmt.Expression(increment)]);
        }

        if (condition == null) condition = new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer != null)
        {
            body = new Stmt.Block([initializer, body]);
        }

        return body;
    }

    private Stmt WhileStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'while'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after condition.");
        var body = Statement();
        return new Stmt.While(condition, body);
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after 'if'.");
        Expr condition = Expression();
        Consume(RIGHT_PAREN, "Expect ')' after if condition.");

        Stmt thenBranch = Statement();
        Stmt? elseBranch = null;
        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }
        Consume(RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    // expression     → assignment ;
    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                return new Expr.Assign(variable.Name, value);
            }
            _ = Error(equals, "Invalid Assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var op = Previous();
            var right = And();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var op = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, op, right);
        }

        return expr;
    }

    // equality       → comparison ( ( "!=" | "==" ) comparison )* ;
    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
    private Expr Comparison()
    {
        var expr = Term();
        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // term           → factor ( ( "-" | "+" ) factor )* ;
    private Expr Term()
    {
        var expr = Factor();
        while (Match(MINUS, PLUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // factor         → unary ( ( "/" | "*" ) unary )* ;
    private Expr Factor()
    {
        var expr = Unary();
        while (Match(SLASH, STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, op, right);
        }

        return expr;
    }

    // unary          → ( "!" | "-" ) unary | primary ;
    private Expr Unary()
    {
        if (!Match(BANG, MINUS))
        {
            return Primary();
        }

        var op = Previous();
        var right = Unary();
        return new Expr.Unary(op, right);

    }

    // primary        → NUMBER | STRING | "true" | "false" | "nil" | IDENTIFIER | "(" expression ")" ;
    private Expr Primary()
    {
        if (Match(FALSE))
        {
            return new Expr.Literal(false);
        }

        if (Match(TRUE))
        {
            return new Expr.Literal(true);
        }

        if (Match(NIL))
        {
            return new Expr.Literal(null);
        }

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }
        throw Error(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        return Check(type) ? Advance() : throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseError();
    }

    private class ParseError : ApplicationException
    {
    }

    private bool Match(params TokenType[] types)
    {
        if (!types.Any(Check))
        {
            return false;
        }

        _ = Advance();
        return true;
    }

    private void Synchronize()
    {
        _ = Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON)
            {
                return;
            }

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
            }

            _ = Advance();
        }
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            _current++;
        }

        return Previous();
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private bool Check(TokenType tokenType)
    {
        return !IsAtEnd() && Peek().Type == tokenType;
    }
}