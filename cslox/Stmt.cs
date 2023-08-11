namespace cslox
{
	internal abstract class Stmt
	{

		internal interface IVisitor<T>
		{
			T VisitExpressionStmt(Expression stmt);
			T VisitPrintStmt(Print stmt);
			T VisitVarStmt(Var stmt);
			T VisitBlockStmt(Block stmt);
		}

		internal abstract T Accept<T>(IVisitor<T> visitor);

		internal class Expression : Stmt
		{
			internal Expression(Expr expr)
			{
				this.expr = expr;
			}

			internal readonly Expr expr;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}
		}

		internal class Print : Stmt
		{
			internal Print(Expr expr)
			{
				this.expr = expr;
			}

			internal readonly Expr expr;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}
		}

		internal class Var : Stmt
		{
			internal Var(Token name, Expr? initializer)
			{
				this.name = name;
				this.initializer = initializer;
			}

			internal readonly Token name;
			internal readonly Expr? initializer;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitVarStmt(this);
			}
		}

		internal class Block : Stmt
		{
			internal Block(List<Stmt> statements)
			{
				this.statements = statements;
			}

			internal readonly List<Stmt> statements;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}
		}
	}
}
