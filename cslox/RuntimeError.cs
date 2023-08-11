namespace cslox
{
    internal class RuntimeError : SystemException
    {
        internal readonly Token token;
        internal RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }
    }
}
