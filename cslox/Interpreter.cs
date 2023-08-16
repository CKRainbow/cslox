namespace cslox
{
    class BreakException : SystemException { }

    internal class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
    {
        Environment environment = new();

        

        internal void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt stmt in statements)
                    Execute(stmt);
            }
            catch (RuntimeError ex)
            {
                Cslox.RuntimeError(ex);
            }
        }

        internal string? Interpret(Expr expr)
        {
            try
            {
                object? value = Evaluate(expr);
                return Stringify(value);
            }
            catch (RuntimeError ex)
            {
                Cslox.RuntimeError(ex);
                return null;
            }
        }

        void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment preEnvironment = this.environment;

            try
            {
                this.environment = environment;
                foreach (Stmt stmt in statements)
                    Execute(stmt);
            }
            finally
            {
                this.environment = preEnvironment;
            }
        }

        object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        public object? VisitBinaryExpr(Expr.Binary expr)
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

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expr);
        }

        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object? VisitUnaryExpr(Expr.Unary expr)
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

        public object? VisitConditionExpr(Expr.Condition expr)
        {
            object? cond = Evaluate(expr.condition);

            if (IsTruthy(cond))
                return Evaluate(expr.thenExpr);
            else
                return Evaluate(expr.elseExpr);
        }

        public object? VisitLogicExpr(Expr.Logic expr)
        {
            object? left = Evaluate(expr.left);
            if (expr.op.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object? VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        public object? VisitAssignExpr(Expr.Assign expr)
        {
            object? value = Evaluate(expr.value);
            environment.Assign(expr.name, value);
            return value;
        }

        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expr);
            return null;
        }

        public object? VisitPrintStmt(Stmt.Print stmt)
        {
            object? value = Evaluate(stmt.expr);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object? VisitIfStmt(Stmt.If stmt)
        {
            object? condition = Evaluate(stmt.condition);
            if (IsTruthy(condition))
                Execute(stmt.thenBranch);
            else if (stmt.elseBranch != null)
                Execute(stmt.elseBranch);
            return null;
        }

        public object? VisitBreakStmt(Stmt.Break stmt)
        {
            throw new BreakException();
        }

        public object? VisitWhileStmt(Stmt.While stmt)
        {
            try
            {
                while (IsTruthy(Evaluate(stmt.condition)))
                    Execute(stmt.body);
            }
            catch (BreakException ex)
            { }

            return null;
        }

        public object? VisitVarStmt(Stmt.Var stmt)
        {
            object? value = null;
            if (stmt.initializer != null)
                value = Evaluate(stmt.initializer);

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
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
            if (value == null) return "nil";

            if (value is double)
            {
                string text = value.ToString() ?? "";
                if (text.EndsWith(".0"))
                    text = text.Substring(0, text.Length - 2);
                return text;
            }

            return value.ToString() ?? "";
        }
    }
}
