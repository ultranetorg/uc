namespace UC.Net.Node.MAUI.Services;

public interface ITransactionsService
{
    Task<CustomCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount);

    Task<CustomCollection<Transaction>> GetLastAsync(int lastTransactionsCount);
}
