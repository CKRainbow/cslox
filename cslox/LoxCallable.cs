namespace cslox
{
    internal interface ILoxCallable
    {
        object? Call(Interpreter interpreter, List<object?> arguments);

        int Arity();
    }
}
