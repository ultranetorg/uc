namespace UC.Net.Node.MAUI.Services;

public class TransactionsService : ITransactionsService
{
    public Task<CustomCollection<TransactionViewModel>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        throw new NotImplementedException();
    }

    public Task<CustomCollection<TransactionViewModel>> GetLastAsync(int lastTransactionsCount)
    {
        throw new NotImplementedException();
    }
}
