using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class AstPrinter : IVisitor<string>
    {
        public static void Main(string[] args)
        {
            Expr expression = new Binary(
                new Unary(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Literal(123)),
                new Token(TokenType.STAR, "*", null, 1),
                new Grouping(
                    new Literal(45.67)));

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(').Append(name);
            foreach (var expr in exprs)
            {
                builder.Append(' ');
                builder.Append(expr.Accept(this));
            }
            builder.Append(')');

            return builder.ToString();
        }

        public string VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Grouping expr)
        {
            return Parenthesize("group", expr.expr);
        }

        public string VisitLiteralExpr(Literal expr)
        {
            return expr.value?.ToString() ?? "nil";
        }

        public string VisitUnaryExpr(Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }
    }
}
