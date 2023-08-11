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
                    "Ternary  : Expr left, Token op1, Expr mid, Token op2, Expr right",
                    "Binary   : Expr left, Token op, Expr right",
                    "Grouping : Expr expr",
                    "Literal  : object value",
                    "Unary    : Token op, Expr right"
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
                sw.WriteLine("internal abstract class {0}", baseName);
                sw.WriteLine("{");
                sw.WriteLine();
                sw.WriteLine("internal abstract T Accept<T>(IVisitor<T> visitor);");
                sw.WriteLine("}");

                // visitor interface 
                sw.WriteLine();
                DefineVisitor(sw, baseName, types);


                // subclass of base class
                foreach (string type in types)
                {
                    sw.WriteLine();
                    string className = type.Split(':')[0].Trim();
                    string fields = type.Split(':')[1].Trim();
                    DefineType(sw, baseName, className, fields);
                }

                sw.WriteLine("}");
            }
        }

        static void DefineType(StreamWriter sw, string baseName, string className, string fieldList)
        {
            // class define
            sw.WriteLine("internal class {0} : {1}", className, baseName);
            sw.WriteLine("{");

            // constructor
            sw.WriteLine("internal {0} ({1})", className, fieldList);
            sw.WriteLine("{");

            string[] fileds = fieldList.Split(',');
            foreach (var field in fileds)
            {
                string fieldName = field.Trim().Split(' ')[1];
                sw.WriteLine("this.{0} = {1};", fieldName, fieldName);
            }

            sw.WriteLine("}");

            sw.WriteLine();

            // fields
            foreach (var field in fileds)
            {
                sw.WriteLine("internal readonly {0};", field.Trim());
            }

            // accept
            sw.WriteLine();
            sw.WriteLine("internal override T Accept<T>(IVisitor<T> visitor)");
            sw.WriteLine("{");

            sw.WriteLine("return visitor.Visit{0}{1}(this);", className, baseName);

            sw.WriteLine("}");

            sw.WriteLine("}");
        }

        static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
        {
            sw.WriteLine("interface IVisitor<T>");
            sw.WriteLine("{");

            foreach (var type in types)
            {
                string typeName = type.Split(':')[0].Trim();
                sw.WriteLine("T Visit{0}{1}({0} {2});", typeName, baseName, baseName.ToLower());
            }

            sw.WriteLine("}");
        }
    }
}