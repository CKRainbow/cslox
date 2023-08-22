using System.Text;

namespace cslox
{
    internal class Cslox
    {
        static readonly Interpreter interpreter = new();

        static bool hasError = false;
        static bool hasRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("cslox Usage: cslox [script]");
                return;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Run(Encoding.UTF8.GetString(bytes));
            // stop processing immediately
            if (hasError)
                return;
            if (hasRuntimeError)
                return;
        }

        private static void RunPrompt()
        {
            while (true)
            {
                hasError = false; // do not stop even there is error;

                Console.Write("> ");
                string? line = Console.ReadLine();

                Scanner scanner = new Scanner(line ?? "");
                List<Token> tokens = scanner.scanTokens();

                Parser parser = new Parser(tokens);
                object? syntax = parser.parseRepl();

                if (hasError) continue;

                if (syntax is List<Stmt>)
                    interpreter.Interpret((List<Stmt>)syntax);
                else if (syntax is Expr)
                {
                    string? result = interpreter.Interpret((Expr)syntax);
                    if (result != null)
                        Console.WriteLine("= {0}", result);
                }



            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (hasError || statements.Count == 0) return;

            interpreter.Interpret(statements);
        }

        internal static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        internal static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
                Report(token.line, " at end", message);
            else
                Report(token.line, String.Format(" at '{0}'", token.lexeme), message);
        }

        internal static void RuntimeError(RuntimeError error)
        {
            Console.Error.WriteLine("{0}\n[ling {1}]", error.Message, error.token.line);
            hasRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error{1}: {2}.", line, where, message);
            hasError = true;
        }
    }
}