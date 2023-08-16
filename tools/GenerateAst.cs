using System.Text;

namespace tools
{
    internal class GenerateAst
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output directory>");
                return;
            }
            string outputDir = args[0];

            DefineAst(
                outputDir, "Expr", new List<string>()
                {
                    "Assign    : Token name, Expr value",
                    "Condition : Expr condition, Expr thenExpr, Expr elseExpr",
                    "Binary    : Expr left, Token op, Expr right",
                    "Logic     : Expr left, Token op, Expr right",
                    "Grouping  : Expr expr",
                    "Literal   : object? value",
                    "Unary     : Token op, Expr right",
                    "Variable  : Token name"
                }
                );

            DefineAst(
                outputDir, "Stmt", new List<string>()
                {
                    "Expression : Expr expr",
                    "Print      : Expr expr",
                    "If         : Expr condition, Stmt thenBranch, Stmt? elseBranch",
                    "Var        : Token name, Expr? initializer",
                    "Block      : List<Stmt> statements",
                    "While      : Expr condition, Stmt body",
                    "Break      : ",
                }
                );
        }

        static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string path = outputDir + '/' + baseName + ".cs";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                sw.WriteLine("namespace cslox");
                sw.WriteLine("{");

                // base class
                sw.WriteLine("\tinternal abstract class {0}", baseName);
                sw.WriteLine("\t{");
                // visitor interface 
                sw.WriteLine();
                DefineVisitor(sw, baseName, types);
                sw.WriteLine();
                sw.WriteLine("\t\tinternal abstract T Accept<T>(IVisitor<T> visitor);");

                // subclass of base class
                foreach (string type in types)
                {
                    sw.WriteLine();
                    string className = type.Split(':')[0].Trim();
                    string fields = type.Split(':')[1].Trim();
                    DefineType(sw, baseName, className, fields);
                }

                sw.WriteLine("\t}");

                sw.WriteLine("}");
            }
        }

        static void DefineType(StreamWriter sw, string baseName, string className, string fieldList)
        {
            // class define
            sw.WriteLine("\t\tinternal class {0} : {1}", className, baseName);
            sw.WriteLine("\t\t{");

            // constructor
            sw.WriteLine("\t\t\tinternal {0}({1})", className, fieldList);
            sw.WriteLine("\t\t\t{");

            string[] fields;
            if (fieldList.Length == 0)
                fields = Array.Empty<string>();
            else
                fields = fieldList.Split(',');
            foreach (var field in fields)
            {
                string fieldName = field.Trim().Split(' ')[1];
                sw.WriteLine("\t\t\t\tthis.{0} = {1};", fieldName, fieldName);
            }

            sw.WriteLine("\t\t\t}");

            sw.WriteLine();

            // fields
            foreach (var field in fields)
            {
                sw.WriteLine("\t\t\tinternal readonly {0};", field.Trim());
            }

            // accept
            sw.WriteLine();
            sw.WriteLine("\t\t\tinternal override T Accept<T>(IVisitor<T> visitor)");
            sw.WriteLine("\t\t\t{");

            sw.WriteLine("\t\t\t\treturn visitor.Visit{0}{1}(this);", className, baseName);

            sw.WriteLine("\t\t\t}");

            sw.WriteLine("\t\t}");
        }

        static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
        {
            sw.WriteLine("\t\tinternal interface IVisitor<T>");
            sw.WriteLine("\t\t{");

            foreach (var type in types)
            {
                string typeName = type.Split(':')[0].Trim();
                sw.WriteLine("\t\t\tT Visit{0}{1}({0} {2});", typeName, baseName, baseName.ToLower());
            }

            sw.WriteLine("\t\t}");
        }
    }
}