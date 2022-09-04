namespace UC.Net.Node.MAUI.Services;

public class TransactionsMockService : ITransactionsService
{
    private readonly IServicesMockData _data;

    public TransactionsMockService(IServicesMockData data)
    {
        _data = data;
    }

    public Task<ObservableCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        Guard.IsNotNull(accountAddress, nameof(accountAddress));
        Guard.IsGreaterThan(lastTransactionsCount, 0, nameof(lastTransactionsCount));

        IEnumerable<Transaction> lastTransactions = _data.Transactions
            .Where(x => string.Equals(x.Account.Address, accountAddress, StringComparison.InvariantCultureIgnoreCase))
            .Take(lastTransactionsCount);
        ObservableCollection<Transaction> result = new(lastTransactions);
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<Transaction>> GetLastAsync(int lastTransactionsCount)
    {
        IEnumerable<Transaction> lastTransactions = _data.Transactions.Take(lastTransactionsCount);
        ObservableCollection<Transaction> result = new(lastTransactions);
        return Task.FromResult(result);
    }
}
