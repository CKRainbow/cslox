namespace cslox
{
	internal abstract class Expr
	{

		internal interface IVisitor<T>
		{
			T VisitAssignExpr(Assign expr);
			T VisitConditionExpr(Condition expr);
			T VisitBinaryExpr(Binary expr);
			T VisitLogicExpr(Logic expr);
			T VisitGroupingExpr(Grouping expr);
			T VisitLiteralExpr(Literal expr);
			T VisitUnaryExpr(Unary expr);
			T VisitVariableExpr(Variable expr);
		}

		internal abstract T Accept<T>(IVisitor<T> visitor);

		internal class Assign : Expr
		{
			internal Assign(Token name, Expr value)
			{
				this.name = name;
				this.value = value;
			}

			internal readonly Token name;
			internal readonly Expr value;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitAssignExpr(this);
			}
		}

		internal class Condition : Expr
		{
			internal Condition(Expr condition, Expr thenExpr, Expr elseExpr)
			{
				this.condition = condition;
				this.thenExpr = thenExpr;
				this.elseExpr = elseExpr;
			}

			internal readonly Expr condition;
			internal readonly Expr thenExpr;
			internal readonly Expr elseExpr;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitConditionExpr(this);
			}
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

		internal class Logic : Expr
		{
			internal Logic(Expr left, Token op, Expr right)
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
				return visitor.VisitLogicExpr(this);
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

		internal class Variable : Expr
		{
			internal Variable(Token name)
			{
				this.name = name;
			}

			internal readonly Token name;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitVariableExpr(this);
			}
		}
	}
}
