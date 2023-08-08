namespace cslox
{
    internal abstract class Expr
    {

        internal abstract T Accept<T>(IVisitor<T> visitor);
    }

    interface IVisitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
    }

    internal class Binary : Expr
    {
        internal Binary(Expr left, Token op, Expr right)
        {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        internal readonly Expr left;
        internal readonly Token op;
        internal readonly Expr right;

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBinaryExpr(this);
        }
    }

    internal class Grouping : Expr
    {
        internal Grouping(Expr expr)
        {
            this.expr = expr;
        }

        internal readonly Expr expr;

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpr(this);
        }
    }

    internal class Literal : Expr
    {
        internal Literal(object? value)
        {
            this.value = value;
        }

        internal readonly object? value;

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLiteralExpr(this);
        }
    }

    internal class Unary : Expr
    {
        internal Unary(Token op, Expr right)
        {
            this.op = op;
            this.right = right;
        }

        internal readonly Token op;
        internal readonly Expr right;

        internal override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpr(this);
        }
    }
}
