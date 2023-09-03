using System.Text;

namespace cslox
{
    internal class AstPrinterRPN : Expr.IVisitor<string>
    {
        public static void Main(string[] args)
        {
            Expr expression = new Expr.Binary(
                new Expr.Unary(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Expr.Literal(123)),
                new Token(TokenType.STAR, "*", null, 1),
                new Expr.Grouping(
                    new Expr.Literal(45.67)));

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

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return expr.Accept(this);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value?.ToString() ?? "nil";
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.op.lexeme, expr.right);
        }

        public string VisitTernaryExpr(Expr.Condition expr)
        {
            return Parenthesize("cond", expr.condition, expr.thenExpr, expr.elseExpr);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            throw new NotImplementedException();
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            throw new NotImplementedException();
        }

        public string VisitConditionExpr(Expr.Condition expr)
        {
            throw new NotImplementedException();
        }

        public string VisitLogicExpr(Expr.Logic expr)
        {
            throw new NotImplementedException();
        }

        public string VisitCallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }
    }
}
