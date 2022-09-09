namespace UC.Net.Node.MAUI.Services;

public class TransactionsService : ITransactionsService
{
    public Task<CustomCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        throw new NotImplementedException();
    }

    public Task<CustomCollection<Transaction>> GetLastAsync(int lastTransactionsCount)
    {
        throw new NotImplementedException();
    }
}
