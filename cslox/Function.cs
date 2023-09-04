namespace cslox
{
    internal class LoxCallable_Function : ILoxCallable
    {
        readonly Stmt.Function declaration;

        readonly Environment enclosure;

        readonly bool isInitializer;

        internal LoxCallable_Function(Stmt.Function declaration, Environment enclosure, bool isInitializer)
        {
            this.declaration = declaration;
            this.enclosure = enclosure;
            this.isInitializer = isInitializer;
        }

        public int Arity()
        {
            return declaration.parameters.Count;
        }

        public object? Call(Interpreter interpreter, List<object?>? arguments)
        {
            Environment environment = new Environment(enclosure);
            if (declaration.parameters != null)
            {
                for (int i = 0; i < arguments?.Count; i++)
                    environment.Define(declaration.parameters[i].lexeme, arguments[i]);
            }
            try
            {
                interpreter.ExecuteBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                //使用空return
                if (isInitializer) return enclosure.GetAt(0, "this");
                return returnValue.value;
            }

            if (isInitializer) return enclosure.GetAt(0, "this");

            return null;
        }

        internal LoxCallable_Function Bind(LoxInstance instance)
        {
            Environment env = new(enclosure);
            env.Define("this", instance);
            return new LoxCallable_Function(declaration, env, isInitializer);
        }

        internal bool IsGetter()
        {
            return declaration.parameters == null;
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

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            return (double)System.DateTime.Now.Millisecond / 1000d;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}
