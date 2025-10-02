using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AstGenerator;

[Generator]
public class AstGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext initContext)
    {
        var expr = DefineAst("Expr", [
            "Assign : Token Name, Expr Value",
            "Binary : Expr Left, Token Operator, Expr Right",
            "Call : Expr Callee, Token Paren, List<Expr> Arguments",
            "Get : Expr Object, Token Name",
            "Grouping : Expr Expression",
            "Literal : object Value",
            "Logical : Expr Left, Token Operator, Expr Right",
            "Set : Expr Object, Token Name, Expr Value",
            "Super : Token Keyword, Token Method",
            "This : Token Keyword",
            "Unary : Token Operator, Expr Right",
            "Variable : Token Name"
        ]);

        var stmt = DefineAst("Stmt", [
            "Block : List<Stmt> Statements",
            "Class : Token Name, Expr.Variable? superclass, List<Stmt.Function> methods",
            "Expression : Expr expression",
            "Function : Token Name, List<Token> Params, List<Stmt> body",
            "If : Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print : Expr expression",
            "Return : Token Keyword, Expr Value",
            "While : Expr condition, Stmt body",
            "Var : Token Name, Expr Initializer"
        ]);

        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Expr.g.cs",
            SourceText.From(expr, Encoding.UTF8)));
        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Stmt.g.cs",
            SourceText.From(stmt, Encoding.UTF8)));
    }

    private string DefineAst(string baseName, List<string> types)
    {
        StringBuilder sb = new();
        sb.AppendLine("namespace dotlox.TreeWalkingInterpreter;\n");
        sb.AppendLine("#nullable enable");
        sb.Append("public abstract class ").Append(baseName).AppendLine(" {");
        // Visitor interface
        DefineVisitor(sb, baseName, types);

        // The Ast Classes
        foreach (var type in types)
        {
            var className = type.Split(':')[0].Trim();
            var fields = type.Split(':')[1].Trim();
            DefineType(sb, baseName, className, fields);
            sb.AppendLine();
        }
        sb.AppendLine("  public abstract R Accept<R>(IVisitor<R> visitor);");
        sb.AppendLine();
        sb.AppendLine("}");
        sb.AppendLine("#nullable disable");
        return sb.ToString();
    }

    private void DefineType(StringBuilder sb, string baseName, string className, string fields)
    {
        sb.Append($"  public class {className} : {baseName}").AppendLine(" {");

        var fieldList = fields.Split(',');

        // Properties
        foreach (var f in fieldList)
        {
            sb.AppendLine($"    public readonly {f.Trim()};");
        }

        // Constructor
        sb.Append($"  public {className}({fields})").AppendLine("{");

        foreach (var f in fieldList)
        {
            var name = f.Trim().Split(' ')[1].Trim();
            sb.AppendLine($"    this.{name} = {name};");
        }
        sb.AppendLine("    }");

        // Override method
        sb.AppendLine();
        sb.AppendLine("  public override R Accept<R> (IVisitor<R> visitor) {");
        sb.AppendLine($"    return visitor.Visit" + className + baseName + "(this);");
        sb.AppendLine("  }");
        sb.AppendLine("  }");
    }

    private void DefineVisitor(StringBuilder sb, string baseName, List<string> types)
    {
        sb.AppendLine("  public interface IVisitor<R> {");

        foreach (var type in types)
        {
            var typeName = type.Split(':')[0].Trim();
            sb.AppendLine($"    R Visit{typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");"}");
        }

        sb.AppendLine("  }");
    }
}
