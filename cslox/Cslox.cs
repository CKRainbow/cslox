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
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }
        }

        private static void runFile(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            run(Encoding.UTF8.GetString(bytes));
            // stop processing immediately
            if (hasError)
                return;
        }

        private static void runPrompt()
        {

            while (true)
            {
                Console.WriteLine("> ");
                string? line = Console.ReadLine();
                if (line == null)
                    break;
                run(line);
                hasError = false; // do not stop even there is error;
            }
        }

        private static void run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        internal static void error(int line, string message)
        {
            report(line, "", message);
        }

        private static void report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error{1}: {2}.", line, where, message);
            hasError = true;
        }
    }
}