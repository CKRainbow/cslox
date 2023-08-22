namespace cslox
{
    internal class Return : SystemException
    {
        internal readonly object? value;

        internal Return(object? value)
        {
            this.value = value;
        }
    }
}
