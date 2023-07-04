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
        var expr = DefineAst("Expr", new List<string>
        {
            "Binary : Expr Left, Token Operator, Expr Right",
            "Grouping : Expr Expression",
            "Literal : object Value",
            "Unary : Token Operator, Expr Right",
        });
        
        initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "Expr.g.cs",
            SourceText.From(expr, Encoding.UTF8)));
    }

    private string DefineAst(string baseName, List<string> types)
    {
        StringBuilder sb = new();
        sb.AppendLine("namespace dotlox;\n");
        sb.AppendLine($$"""public abstract class {{baseName}} {""");
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