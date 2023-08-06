namespace cslox
{
    internal class Scanner
    {
        readonly string source;
        readonly List<Token> tokens = new List<Token>();

        int start = 0;
        int current = 0;
        int line = 1;

        internal Scanner(string source)
        {
            this.source = source;
        }

        internal List<Token> scanTokens()
        {
            while (!isAtEnd())
            {
                start = current;
                scanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        bool isAtEnd()
        {
            return current >= source.Length;
        }

        void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(':
                    addToken(TokenType.LEFT_PAREN);
                    break;
                case ')':
                    addToken(TokenType.RIGHT_PAREN);
                    break;
                case '{':
                    addToken(TokenType.LEFT_BRACE);
                    break;
                case '}':
                    addToken(TokenType.RIGHT_BRACE);
                    break;
                case ',':
                    addToken(TokenType.COMMA);
                    break;
                case '.':
                    addToken(TokenType.DOT);
                    break;
                case '-':
                    addToken(TokenType.MINUS);
                    break;
                case '+':
                    addToken(TokenType.PLUS);
                    break;
                case ';':
                    addToken(TokenType.SEMICOLON);
                    break;
                case '*':
                    addToken(TokenType.STAR);
                    break;
                case '!':
                    addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '>':
                    addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '<':
                    addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '/':
                    if (match('/'))
                    {
                        // a comment goes until a line ends
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else
                    {
                        addToken(TokenType.SLASH);
                    }
                    break;

                case ' ':
                case '\t':
                case '\r':
                    break;
                case '\n':
                    line++;
                    break;

                default:
                    Cslox.error(line, "Unexpected character.");
                    break;
            }
        }

        char advance()
        {
            current++;
            return source[current - 1];
        }

        bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        char peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        void addToken(TokenType type)
        {
            addToken(type, null);
        }

        void addToken(TokenType type, object? literal)
        {
            string text = source.Substring(start, current);
            tokens.Add(new Token(type, text, literal, line));
        }
    }
}
