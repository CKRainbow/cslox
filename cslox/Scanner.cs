using System.Collections;

namespace cslox
{
    internal class Scanner
    {
        readonly string source;
        readonly List<Token> tokens = new ();

        static readonly Dictionary<string, TokenType> keywords = new ();

        static Scanner()
        {
            keywords.Add("and", TokenType.AND);
            keywords.Add("or", TokenType.OR);
            keywords.Add("class", TokenType.CLASS);
            keywords.Add("else", TokenType.ELSE);
            keywords.Add("false", TokenType.FALSE);
            keywords.Add("for", TokenType.FOR);
            keywords.Add("fun", TokenType.FUN);
            keywords.Add("if", TokenType.IF);
            keywords.Add("nil", TokenType.NIL);
            keywords.Add("print", TokenType.PRINT);
            keywords.Add("return", TokenType.RETURN);
            keywords.Add("super", TokenType.SUPER);
            keywords.Add("this", TokenType.THIS);
            keywords.Add("true", TokenType.TRUE);
            keywords.Add("var", TokenType.VAR);
            keywords.Add("while", TokenType.WHILE);
        }

        int start = 0;
        int current = 0;
        int line = 1;

        int commentNum = 0;

        internal Scanner(string source)
        {
            this.source = source;
        }

        internal List<Token> scanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        bool IsAtEnd()
        {
            return current >= source.Length;
        }

        void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                //characters
                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    AddToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(TokenType.COMMA);
                    break;
                case '.':
                    AddToken(TokenType.DOT);
                    break;
                case '-':
                    AddToken(TokenType.MINUS);
                    break;
                case '+':
                    AddToken(TokenType.PLUS);
                    break;
                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;
                case '*':
                    AddToken(TokenType.STAR);
                    break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // a comment goes until a line ends
                        while (Peek() != '\n' && !IsAtEnd()) Advance(); // use peek() for looking ahead for '\n' to update line
                    }
                    else if (Match('*'))
                    {
                        MultilineComment();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                // special characters
                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    line++;
                    break;

                // string
                case '"':
                    LiteralString();
                    break;

                // numbers keywords and identifier
                default:
                    if (IsDigit(c))
                        LiteralNumber();
                    else if (IsAlpha(c))
                        Identifier();
                    else
                        Cslox.Error(line, "Unexpected character.");
                    break;
            }
        }

        char Advance()
        {
            current++;
            return source[current - 1];
        }

        bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        void AddToken(TokenType type, object? literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c == '_');
        }

        bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        void LiteralString() {
            while(Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Cslox.Error(line, "Unterminated string.");
                return;
            }

            Advance();

            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }

        void LiteralNumber()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, float.Parse(source.Substring(start, current - start)));
        }

        void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = source.Substring(start, current - start);
            if (!keywords.TryGetValue(text, out TokenType type))
                type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        void MultilineComment()
        {
            commentNum++;
            while(commentNum > 0 && !IsAtEnd())
            {
                if (Peek() == '/' && PeekNext() == '*')
                {
                    commentNum++;
                    Advance();
                    Advance();
                }
                else if (Peek() == '*' && PeekNext() == '/')
                {
                    commentNum--;
                    Advance();
                    Advance();
                } else
                {
                    if (Peek() == '\n')
                        line++;
                    Advance();
                }
            }
        }
    }
}
