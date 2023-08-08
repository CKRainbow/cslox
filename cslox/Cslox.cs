using System.Text;

namespace cslox
{
    internal class Cslox
    {
        static bool hasError = false;


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
        }

        private static void RunPrompt()
        {

            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();
                if (line == null)
                    break;
                Run(line);
                hasError = false; // do not stop even there is error;
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            Parser parser = new Parser(tokens);
            Expr? expression = parser.Parse();

            if (hasError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
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

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error{1}: {2}.", line, where, message);
            hasError = true;
        }
    }
}