using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
    {
        readonly Interpreter interpreter;

        readonly List<Dictionary<string, bool>> scopes = new();

        FunctionType currentFunction = FunctionType.NONE; 

        internal Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        enum FunctionType
        {
            NONE,
            FUNCTION
        }

        internal void Resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        void ResolveLocal(Expr expr, Token name)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    return;
                }
            }
        }

        void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (Token param in function.parameters)
            {
                Declare(param);
                Define(param);
            }

            //静态分析中立刻遍历函数体
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        void BeginScope()
        {
            scopes.Append(new Dictionary<string, bool>());
        }

        void EndScope()
        {
            scopes.Last();
        }

        void Declare(Token name)
        {
            if (scopes.Count == 0)
                return;

            Dictionary<string, bool> scope = scopes.Last();

            if (scope.ContainsKey(name.lexeme))
                Cslox.Error(name, "Already declared variable with this name in this scope");

            //false 指该变量尚未就绪（是否已经结束了对变量初始化式的解析）
            scope.Add(name.lexeme, false);
        }

        void Define(Token name)
        {
            if (scopes.Count == 0)
                return;

            //已完全初始化并可用
            scopes.Last().Add(name.lexeme, true);
        }

        public object? VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object? VisitBreakStmt(Stmt.Break stmt)
        {
            return null;
        }

        public object? VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach(var arg in expr.arguments)
            {
                Resolve(arg);
            }

            return null;
        }

        public object? VisitConditionExpr(Expr.Condition expr)
        {
            Resolve(expr.condition);
            Resolve(expr.thenExpr);
            Resolve(expr.elseExpr);
            return null;
        }

        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expr);
            return null;
        }

        public object? VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            //在函数体中调用自身递归
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expr);
            return null;
        }

        public object? VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object? VisitLogicExpr(Expr.Logic expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object? VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expr);
            return null;
        }

        public object? VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
                Cslox.Error(stmt.keyword, "Cannot return from top-level code");
            if (stmt.value != null)
                Resolve(stmt.value);
            return null;
        }

        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object? VisitVariableExpr(Expr.Variable expr)
        {
            //变量是否在其自身的初始化式中被访问
            if (scopes.Count > 0 && scopes.Last().TryGetValue(expr.name.lexeme, out bool init) && init == false) 
            {
                Cslox.Error(expr.name, "Cannot read local variable in its own initializer");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        public object? VisitVarStmt(Stmt.Var stmt)
        {
            //先声明
            Declare(stmt.name);
            //初始化
            if (stmt.initializer != null)
                Resolve(stmt.initializer);
            //后定义
            Define(stmt.name);

            return null;
        }

        public object? VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }
    }
}
