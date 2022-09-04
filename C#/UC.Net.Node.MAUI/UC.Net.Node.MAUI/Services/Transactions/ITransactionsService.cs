namespace UC.Net.Node.MAUI.Services;

public interface ITransactionsService
{
    Task<ObservableCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount);

    Task<ObservableCollection<Transaction>> GetLastAsync(int lastTransactionsCount);
}
