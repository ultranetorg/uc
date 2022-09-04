namespace UC.Net.Node.MAUI.Services;

public class TransactionsService : ITransactionsService
{
    public Task<ObservableCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        throw new System.NotImplementedException();
    }

    public Task<ObservableCollection<Transaction>> GetLastAsync(int lastTransactionsCount)
    {
        throw new System.NotImplementedException();
    }
}
