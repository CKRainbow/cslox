using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class AstPrinterRPN : IVisitor<string>
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

            Console.WriteLine(new AstPrinterRPN().Print(expression));
        }

        internal string Print(Expr? expr)
        {
            if (expr == null) return "";
            return expr.Accept(this);
        }

        string Parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var expr in exprs)
            {
                builder.Append(expr.Accept(this));
                builder.Append(' ');
            }
            builder.Append(name);

            return builder.ToString();
        }

        public string VisitBinaryExpr(Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Grouping expr)
        {
            return expr.Accept(this);
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
