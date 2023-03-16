namespace Ardalis.GuardClauses;

public static class GuardClauseExtensions
{
    public static void Null<TException>(this IGuardClause guardClause, object input, string message = null)
        where TException : Exception
    {
        if (input == null)
        {
            TException exception = (TException) Activator.CreateInstance(typeof(TException), message);
            throw exception;
        }
    }
}
