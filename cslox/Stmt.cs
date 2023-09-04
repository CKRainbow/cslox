namespace cslox
{
	internal abstract class Stmt
	{

		internal interface IVisitor<T>
		{
			T VisitExpressionStmt(Expression stmt);
			T VisitPrintStmt(Print stmt);
			T VisitIfStmt(If stmt);
			T VisitVarStmt(Var stmt);
			T VisitBlockStmt(Block stmt);
			T VisitWhileStmt(While stmt);
			T VisitBreakStmt(Break stmt);
			T VisitFunctionStmt(Function stmt);
			T VisitReturnStmt(Return stmt);
			T VisitClassStmt(Class stmt);
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

		internal class If : Stmt
		{
			internal If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
			{
				this.condition = condition;
				this.thenBranch = thenBranch;
				this.elseBranch = elseBranch;
			}

			internal readonly Expr condition;
			internal readonly Stmt thenBranch;
			internal readonly Stmt? elseBranch;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitIfStmt(this);
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

		internal class While : Stmt
		{
			internal While(Expr condition, Stmt body)
			{
				this.condition = condition;
				this.body = body;
			}

			internal readonly Expr condition;
			internal readonly Stmt body;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitWhileStmt(this);
			}
		}

		internal class Break : Stmt
		{
			internal Break()
			{
			}


			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitBreakStmt(this);
			}
		}

		internal class Function : Stmt
		{
			internal Function(Token name, List<Token>? parameters, List<Stmt> body)
			{
				this.name = name;
				this.parameters = parameters;
				this.body = body;
			}

			internal readonly Token name;
			internal readonly List<Token>? parameters;
			internal readonly List<Stmt> body;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitFunctionStmt(this);
			}
		}

		internal class Return : Stmt
		{
			internal Return(Token keyword, Expr? value)
			{
				this.keyword = keyword;
				this.value = value;
			}

			internal readonly Token keyword;
			internal readonly Expr? value;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitReturnStmt(this);
			}
		}

		internal class Class : Stmt
		{
			internal Class(Token name, List<Stmt.Function> methods, List<Stmt.Function> staticMethods)
			{
				this.name = name;
				this.methods = methods;
				this.staticMethods = staticMethods;
			}

			internal readonly Token name;
			internal readonly List<Stmt.Function> methods;
			internal readonly List<Stmt.Function> staticMethods;

			internal override T Accept<T>(IVisitor<T> visitor)
			{
				return visitor.VisitClassStmt(this);
			}
		}
	}
}
