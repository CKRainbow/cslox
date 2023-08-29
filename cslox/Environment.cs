namespace cslox
{
    internal class Environment
    {
        readonly Environment? enclosing;

        readonly Dictionary<string, object?> values = new();

        internal Environment()
        {
            enclosing = null;
        }

        internal Environment(Environment? enclosing)
        {
            this.enclosing = enclosing;
        }

        Environment Ancestor(int distance)
        {
            Environment env = this;
            for (int i = 0; i < distance; i++)
            {
                env = env.enclosing??env;
            }

            return env;
        }

        internal object? Get(Token name)
        {
            if (values.TryGetValue(name.lexeme, out var value))
                return value;

            if (enclosing != null) return enclosing.Get(name);

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'");
        }

        internal object? GetAt(int distance, string name)
        {
            return Ancestor(distance).values.GetValueOrDefault(name);
        }

        internal void Define(string name, object? value)
        {
            values[name] = value;
        }

        internal void Assign(Token name, object? value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'");
        }

        internal void AssignAt(int distance, string name, object? value)
        {
            Ancestor(distance).values.Add(name, value);
        }
    }
}
