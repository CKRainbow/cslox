using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class Resolver : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
    {
        class Variable
        {
            internal readonly Token name;
            internal VariableState state;
        
            internal Variable(Token name, VariableState state)
            {
                this.name = name;
                this.state = state;
            }
        }

        enum VariableState
        {
            DECLARED,
            DEFINED,
            READ,
        }

        readonly Interpreter interpreter;

        readonly List<Dictionary<string, Variable>> scopes = new();

        readonly HashSet<Expr> unusedVariables = new();

        FunctionType currentFunction = FunctionType.NONE;

        ClassType currentClass = ClassType.NONE;

        internal Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        enum FunctionType
        {
            NONE,
            FUNCTION,
            METHOD,
            INITIALIZER,
        }

        enum ClassType
        {
            NONE,
            CLASS,
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

        void ResolveLocal(Expr expr, Token name, bool isRead)
        {
            for (int i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i].ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, scopes.Count - 1 - i);
                    if (isRead)
                        scopes.Last()[name.lexeme] = new Variable(name, VariableState.READ);      
                }
            }
        }

        void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            if (function.parameters != null)
            {
                foreach (Token param in function.parameters)
                {
                    Declare(param);
                    Define(param);
                }
            }
            //静态分析中立刻遍历函数体
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingFunction;
        }

        void BeginScope()
        {
            scopes.Add(new Dictionary<string, Variable>());
        }

        void EndScope()
        {
            var scope = scopes[scopes.Count-1];
            scopes.Remove(scope);

            foreach(var pair in scope)
            {
                if (pair.Value.state == VariableState.DEFINED)
                    Cslox.Error(pair.Value.name, "Local variable is not read");
            }
        }

        void Declare(Token name)
        {
            if (scopes.Count == 0)
                return;

            Dictionary<string, Variable> scope = scopes.Last();

            if (scope.ContainsKey(name.lexeme))
                Cslox.Error(name, "Already declared variable with this name in this scope");

            //false 指该变量尚未就绪（是否已经结束了对变量初始化式的解析）
            scope[name.lexeme] = new Variable(name, VariableState.DECLARED);
        }

        void Define(Token name)
        {
            if (scopes.Count == 0)
                return;

            //已完全初始化并可用
            scopes.Last()[name.lexeme] = new Variable(name, VariableState.DEFINED);
        }

        public object? VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr);
            ResolveLocal(expr, expr.name, false);
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
            {
                if (currentFunction == FunctionType.INITIALIZER)
                    Cslox.Error(stmt.keyword, "Canont return a value from an initializer");
                Resolve(stmt.value);
            }
                
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
            if (scopes.Count > 0 && scopes.Last().TryGetValue(expr.name.lexeme, out Variable? variable) && variable?.state == VariableState.DECLARED) 
            {
                Cslox.Error(expr.name, "Cannot read local variable in its own initializer");
            }

            ResolveLocal(expr, expr.name, true);
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

        public object? VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;
            Declare(stmt.name);
            Define(stmt.name);
            BeginScope();
            //token应该用什么？
            scopes.Last()["this"] = new(stmt.name, VariableState.READ);
            foreach(var method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init")
                    declaration = FunctionType.INITIALIZER;
                ResolveFunction(method, declaration);
            }
            foreach(var staticMethod in stmt.staticMethods)
            {
                BeginScope();
                scopes.Last()["this"] = new(stmt.name, VariableState.READ);
                ResolveFunction(staticMethod, FunctionType.METHOD);
                EndScope();
            }
            EndScope();
            currentClass = enclosingClass;
            return null;
        }

        public object? VisitGetExpr(Expr.Get expr)
        {
            //字段属性为动态查找
            Resolve(expr._object);
            return null;
        }

        public object? VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr._object);
            return null;
        }

        public object? VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
                Cslox.Error(expr.keyword, "Cannot use 'this' outside of a class");
            ResolveLocal(expr, expr.keyword, true);
            return null;
        }
    }
}
