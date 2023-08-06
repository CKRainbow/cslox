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

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        internal static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line {0}] Error{1}: {2}.", line, where, message);
            hasError = true;
        }
    }
}