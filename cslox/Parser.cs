namespace cslox
{
    internal class ParseError : SystemException { }

    internal class Parser
    {


        readonly List<Token> tokens;
        int current = 0;

        bool allowExpression;
        bool foundExpression = false;

        internal Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        internal List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
                statements.Add(Declaration());

            return statements;
        }

        internal object? parseRepl()
        {
            allowExpression = true;
            List<Stmt> statements = new();
            while(!IsAtEnd())
            {
                statements.Add(Declaration());

                if (foundExpression)
                {
                    Stmt last = statements.Last();
                    return ((Stmt.Expression)last).expr;
                }

                allowExpression = false;
            }

            return statements;
        }

        // program -> declaration* EOF


        // declaration -> varDecl | statement
        Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch(ParseError ex)
            {
                Synchronize();
                return null;
            }
            
        }
        

        // varDecl -> "var" IDENTIFIER ( "=" expression )? ";"
        Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "");

            Expr? initializer = null;
            if (Match(TokenType.EQUAL)) initializer = ExpressionExpr();

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration");

            return new Stmt.Var(name, initializer);
        }


        // statement -> exprStmt | printStmt | block;
        Stmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatment();

            if (Match(TokenType.LEFT_BRACE)) return Block();

            return ExpressionStatement();
        }

        // block -> "{" declaration* "}"
        Stmt Block()
        {
            List<Stmt> statements = new();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
                statements.Add(Declaration());

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block");
            return new Stmt.Block(statements);
        }

        // exprStmt -> expression ";"
        Stmt ExpressionStatement()
        {
            Expr expr = ExpressionExpr();

            if (allowExpression && IsAtEnd())
                foundExpression = true;
            else
                Consume(TokenType.SEMICOLON, "Expect ';' after value");

            return new Stmt.Expression(expr);
        }

        // printStmt -> "print" expression ";"
        Stmt PrintStatment()
        {
            Expr expr = ExpressionExpr();

            Consume(TokenType.SEMICOLON, "Expect ';' after value");

            return new Stmt.Print(expr);
        }

        // expression -> comma
        Expr ExpressionExpr()
        {
            return CommaExpr();
        }

        // comma -> assignment ( "," assignment )*
        Expr CommaExpr()
        {
            Expr expr = AssignmentExpr();
            while (Match(TokenType.COMMA))
            {
                Token op = Previous();
                Expr right = AssignmentExpr();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        // assignment -> IDENTIFIER "=" assignment | conditional
        Expr AssignmentExpr()
        {
            Expr expr = ConditionalExpr();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = AssignmentExpr();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target");
            }

            return expr;
        }


        // conditional -> equality ( "?" expression ":" conditional )?
        Expr ConditionalExpr()
        {
            Expr expr = EqualityExpr();
            if (Match(TokenType.QUESTION))
            {
                var op1 = Previous();
                Expr mid = ExpressionExpr();
                Consume(TokenType.COLON, "Expect ':' after then branch of conditional expression");
                var op2 = Previous();
                Expr right = ConditionalExpr();
                expr = new Expr.Ternary(expr, op1, mid, op2, right);
            }
            return expr;
        }

        // equality -> comarison ( ( "!=" | "==" ) comparison )*
        Expr EqualityExpr()
        {
            Expr expr = ComparisonExpr();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = ComparisonExpr();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        // comarison -> term ( ( ">" | ">=" | "<" | "<=" | ) term )*
        Expr ComparisonExpr()
        {
            Expr expr = TermExpr();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = TermExpr();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        // term -> factor ( ( "+" | "-" ) factor )*
        Expr TermExpr()
        {
            Expr expr = FactorExpr();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = FactorExpr();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        // factor -> unary ( ( "*" | "/" ) unary )*
        Expr FactorExpr()
        {
            Expr expr = UnaryExpr();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = UnaryExpr();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        // unary -> ( "!" | "-" ) unary | primary
        Expr UnaryExpr()
        {

            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr expr = UnaryExpr();
                return new Expr.Unary(op, expr);
            }

            return PrimaryExpr();
        }

        // primary -> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER
        // error production for left expr missing binary expression
        // + * / > >= < <= == != ,
        Expr PrimaryExpr()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Expr.Literal(Previous().literal);
            if (Match(TokenType.IDENTIFIER))
                return new Expr.Variable(Previous());

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = ExpressionExpr();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            // error productions
            if (Match(TokenType.COMMA))
            {
                Error(Previous(), "Missing left-hand operand");
                CommaExpr();
                return null;
            }
            else if (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Error(Previous(), "Missing left-hand operand");
                EqualityExpr();
                return null;
            }
            else if (Match(TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS_EQUAL, TokenType.LESS))
            {
                Error(Previous(), "Missing left-hand operand");
                ComparisonExpr();
                return null;
            }
            else if (Match(TokenType.PLUS))
            {
                Error(Previous(), "Missing left-hand operand");
                TermExpr();
                return null;
            }
            else if (Match(TokenType.SLASH, TokenType.STAR))
            {
                Error(Previous(), "Missing left-hand operand");
                FactorExpr();
                return null;
            }

            throw Error(Peek(), "Expect expression");
        }


        bool Match(params TokenType[] types)
        {
            foreach (TokenType t in types)
            {
                if (Check(t))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().type == type;
        }

        bool IsAtEnd()
        {
            return Peek().type == TokenType.EOF;
        }

        Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }

        Token Previous()
        {
            return tokens[current - 1];
        }

        Token Peek()
        {
            return tokens[current];
        }

        Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        static ParseError Error(Token token, string message)
        {
            Cslox.Error(token, message);
            return new ParseError();
        }

        void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().type == TokenType.SEMICOLON) return;

                switch (Peek().type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}
