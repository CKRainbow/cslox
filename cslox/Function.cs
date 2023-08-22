namespace cslox
{
    internal class LoxCallable_Function : ILoxCallable
    {
        readonly Stmt.Function declaration;

        readonly Environment enclosure;

        internal LoxCallable_Function(Stmt.Function declaration, Environment enclosure)
        {
            this.declaration = declaration;
            this.enclosure = enclosure;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object? call(Interpreter interpreter, List<object?> arguments)
        {
            Environment environment = new Environment(enclosure);
            for (int i = 0; i < arguments.Count; i++)
            {
                environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                return returnValue.value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {declaration.name.lexeme}>";
        }
    }

    internal class LoxCallable_Clock : ILoxCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object? call(Interpreter interpreter, List<object?> arguments)
        {
            return (double)System.DateTime.Now.Millisecond / 1000d;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
