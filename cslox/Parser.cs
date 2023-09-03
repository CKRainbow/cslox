namespace cslox
{
    internal class ParseError : SystemException { }

    internal class Parser
    {


        readonly List<Token> tokens;
        int current = 0;

        bool allowExpression;
        bool foundExpression = false;

        int loopDepth = 0;

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
            while (!IsAtEnd())
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


        // declaration -> funcDecl | varDecl | classDecl | statement
        Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                if (Match(TokenType.CLASS)) return ClassDeclaration();

                if (Match(TokenType.FUN)) return Function("function");

                return Statement();
            }
            catch (ParseError ex)
            {
                Synchronize();
                return null;
            }

        }

        // funcDecl -> "fun" function
        // function -> IDENTIFIER "(" parameters? ")" block
        // parameters -> IDENTIFIER ( "," IDENTIFIER )* 
        Stmt.Function Function(string kind)
        {
            Token name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name");

            Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name");

            List<Token> parameters = new();
            if (!Check(TokenType.RIGHT_PAREN))
                do
                {
                    if (parameters.Count >= 255)
                        Error(Peek(), "Cannot have more than 255 parameters");
                    parameters.Add(Consume(TokenType.IDENTIFIER, $"Expect parameter name"));
                } while (Match(TokenType.COMMA));

            Consume(TokenType.RIGHT_PAREN, $"Expect ')' after parameters");

            Consume(TokenType.LEFT_BRACE, $"Expect '{{' before {kind} body");
            List<Stmt> body = Block();

            return new Stmt.Function(name, parameters, body);
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

        // classDecl -> "class" IDENTIFIER "{" function* "}"
        // 可以自由地向其中加入字段，而非预先定义
        Stmt ClassDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect class name");

            Consume(TokenType.LEFT_BRACE, "Expect '{' before class body");

            List<Stmt.Function> methods = new();
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
                methods.Add(Function("method"));

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body");

            return new Stmt.Class(name, methods);
        }


        // statement -> exprStmt | printStmt | ifStmt | whileStmt | forStmt | returnStmt | block;
        Stmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatment();

            if (Match(TokenType.IF)) return IfStatement();

            if (Match(TokenType.WHILE)) return WhileStatement();

            if (Match(TokenType.FOR)) return ForStatement();

            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            if (Match(TokenType.BREAK)) return BreakStatement();

            if (Match(TokenType.RETURN)) return ReturnStatement();

            return ExpressionStatement();
        }

        // block -> "{" declaration* "}"
        List<Stmt> Block()
        {
            List<Stmt> statements = new();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
                statements.Add(Declaration());

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block");
            return statements;
        }

        Stmt BreakStatement()
        {
            if (loopDepth == 0)
                Error(Previous(), "Must be inside a loop to use 'break'");
            Consume(TokenType.SEMICOLON, "Expect ';' after break");

            return new Stmt.Break();
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

        // ifStmt -> "if" "(" expression ")" statement ( "else" statement )?
        Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after if");
            Expr condition = ExpressionExpr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");

            Stmt thenBranch = Statement();
            Stmt? elseBranch = null;
            if (Match(TokenType.ELSE))
                elseBranch = Statement();

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        // whileStmt -> "while" "(" expression ")" statement
        Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after while");
            Expr condition = ExpressionExpr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition");

            try
            {
                loopDepth++;
                Stmt body = Statement();
                return new Stmt.While(condition, body);
            }
            finally
            {
                loopDepth--;
            }

        }

        // forStmt -> "for" "(" ( varDecl | exprStmt | ";" ) expression? ";" expression? ")" statement
        // 使用语法脱糖，前端接收使用了语法糖的代码，并将其转换成后端知道如何执行的更原始的形式。
        Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after for");
            Stmt? initializer;

            if (Match(TokenType.SEMICOLON))
                initializer = null;
            else if (Match(TokenType.VAR))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();

            Expr? condition = null;
            if (!Check(TokenType.SEMICOLON))
                condition = ExpressionExpr();
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition");

            Expr? increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
                increment = ExpressionExpr();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses");

            try
            {
                loopDepth++;

                Stmt body = Statement();

                // De-sugaring starts
                if (increment != null)
                    body = new Stmt.Block(new List<Stmt> { body, new Stmt.Expression(increment) });
                if (condition == null)
                    condition = new Expr.Literal(true);
                body = new Stmt.While(condition, body);
                if (initializer != null)
                    body = new Stmt.Block(new List<Stmt> { initializer, body });
                // De-sugaring ends

                return body;
            }
            finally
            {
                loopDepth--;
            }
        }

        // returnStmt -> "return" expression? ";"
        Stmt.Return ReturnStatement()
        {
            Token keyword = Previous();
            Expr? value = null;
            if (!Check(TokenType.SEMICOLON))
                value = ExpressionExpr();

            Consume(TokenType.SEMICOLON, $"Expecr ';' after return value");

            return new Stmt.Return(keyword, value);
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

        // assignment -> ( call "." )? IDENTIFIER "=" assignment | conditional
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
                else if (expr is Expr.Get)
                {
                    Expr.Get get = (Expr.Get)expr;
                    //将最后一层get替换为set
                    return new Expr.Set(get._object, get.name, value);
                }

                Error(equals, "Invalid assignment target");
            }

            return expr;
        }


        // conditional -> logic_or ( "?" expression ":" conditional )?
        Expr ConditionalExpr()
        {
            Expr expr = LogicOrExpr();
            if (Match(TokenType.QUESTION))
            {
                Expr thenExpr = ExpressionExpr();
                Consume(TokenType.COLON, "Expect ':' after then branch of conditional expression");
                Expr elseExpr = ConditionalExpr();
                expr = new Expr.Condition(expr, thenExpr, elseExpr);
            }
            return expr;
        }

        // logic_or -> logic_and ( "or" logic_and )*
        Expr LogicOrExpr()
        {
            Expr expr = LogicAndExpr();
            while (Match(TokenType.OR))
            {
                Token op = Previous();
                Expr right = LogicAndExpr();
                expr = new Expr.Logic(expr, op, right);
            }

            return expr;
        }

        // logic_and -> equality ( "and" equality )*
        Expr LogicAndExpr()
        {
            Expr expr = EqualityExpr();
            while (Match(TokenType.AND))
            {
                Token op = Previous();
                Expr right = EqualityExpr();
                expr = new Expr.Logic(expr, op, right);
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

        // unary -> ( "!" | "-" ) unary | call
        Expr UnaryExpr()
        {

            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr expr = UnaryExpr();
                return new Expr.Unary(op, expr);
            }

            return CallExpr();
        }

        // call -> primary ( "(" arguments? ")" | "." INDENTIFIER )*
        Expr CallExpr()
        {
            Expr expr = PrimaryExpr();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                    expr = FinishCallExpr(expr);
                else if (Match(TokenType.DOT))
                {
                    Token name = Consume(TokenType.IDENTIFIER, "Expect property name after '.'");
                    expr = new Expr.Get(expr, name);
                }    
                else
                    break;
            }

            return expr;
        }

        // arguments -> assignment ( "," assignment )*
        Expr FinishCallExpr(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
                do
                {
                    // 报错但不抛出
                    if (arguments.Count >= 255)
                        Error(Peek(), "Cannot have more than 255 arguments");
                    arguments.Add(AssignmentExpr());
                } while (Match(TokenType.COMMA));

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments");

            return new Expr.Call(callee, paren, arguments);
        }

        // primary -> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" | IDENTIFIER
        // error production for left expr missing binary expression
        // + * / > >= < <= == != ,
        Expr PrimaryExpr()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);
            if (Match(TokenType.THIS)) return new Expr.This(Previous());

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
