namespace cslox
{
    internal interface ILoxCallable
    {
        object? call(Interpreter interpreter, List<object?> arguments);

        int Arity();
    }
}
