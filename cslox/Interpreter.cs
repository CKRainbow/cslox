namespace cslox
{
    internal class Interpreter : IVisitor<object?>
    {
        internal void Interpret(Expr expression)
        {
            try
            {
                object? value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError ex)
            {
                Cslox.RuntimeError(ex);
            }
        }

        object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        object? IVisitor<object?>.VisitBinaryExpr(Binary expr)
        {
            object? left = Evaluate(expr.left);
            object? right = Evaluate(expr.right);

            if (left == null || right == null)
                return null;

            switch (expr.op.type)
            {
                case TokenType.PLUS:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    else if (left is string && right is string)
                        return (string)left + (string)right;
                    else if (left is string)
                        return (string)left + right.ToString();
                    else if (right is string)
                        return left.ToString() + (string)right;
                    throw new RuntimeError(expr.op, "Operands msut be two numbers or two strings");
                case TokenType.MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left + (double)right;
                case TokenType.SLASH:
                    CheckNumberOperands(expr.op, left, right);
                    CheckValidDivide(expr.op, (double)right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case TokenType.GREATER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.COMMA:
                    return right;
            }
            return null;
        }

        object? IVisitor<object?>.VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.expr);
        }

        object? IVisitor<object?>.VisitLiteralExpr(Literal expr)
        {
            return expr.value;
        }

        object? IVisitor<object?>.VisitUnaryExpr(Unary expr)
        {
            object? right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            return null;
        }

        object? IVisitor<object?>.VisitTernaryExpr(Ternary expr)
        {
            object? cond = Evaluate(expr.left);

            if (IsTruthy(cond))
                return Evaluate(expr.mid);
            else
                return Evaluate(expr.right);
        }

        bool IsTruthy(object? value)
        {
            if (value == null)
                return false;
            if (value is bool boolean)
                return boolean;
            return true;
        }

        bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        void CheckNumberOperand(Token op, object? operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number");
        }

        void CheckNumberOperands(Token op, object? left, object? right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(op, "Operands must be numbers");
        }

        void CheckValidDivide(Token op, double right)
        {
            if (right == 0d)
                throw new RuntimeError(op, "Cannot divide by zero.");
        }

        string Stringify(object? value)
        {
            if (value == null)
                return "nil";

            if (value is double)
            {
                string text = value.ToString();
                if (text.EndsWith(".0"))
                    text = text.Substring(0, text.Length - 2);
                return text;
            }

            return value.ToString();
        }
    }
}
