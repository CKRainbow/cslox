using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class ParseError : SystemException { }

    internal class Parser
    {
        

        readonly List<Token> tokens;
        int current = 0;

        internal Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        internal Expr? Parse()
        {
            try
            {
                return ExpressionExpr();
            }
            catch (ParseError e)
            {
                return null;
            }
        }

        Expr ExpressionExpr()
        {
            return EqualityExpr();
        }

        Expr EqualityExpr()
        {
            Expr expr = ComparisonExpr();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = ComparisonExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        Expr ComparisonExpr()
        {
            Expr expr = TermExpr();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = TermExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        Expr TermExpr()
        {
            Expr expr = FactorExpr();

            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                Token op = Previous();
                Expr right = FactorExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        Expr FactorExpr()
        {
            Expr expr = UnaryExpr();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token op = Previous();
                Expr right = UnaryExpr();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        Expr UnaryExpr()
        {
            
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token op = Previous();
                Expr expr = UnaryExpr();
                return new Unary(op, expr);
            }

            return PrimaryExpr();
        }

        Expr PrimaryExpr()
        {
            if (Match(TokenType.FALSE)) return new Literal(false);
            if (Match(TokenType.TRUE)) return new Literal(true);
            if (Match(TokenType.NIL)) return new Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING))
                return new Literal(Previous().literal);

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = ExpressionExpr();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
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

            while(!IsAtEnd())
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
