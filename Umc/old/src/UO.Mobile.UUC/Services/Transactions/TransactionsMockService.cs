using UO.Mobile.UUC.Models.Transactions;

namespace UO.Mobile.UUC.Services.Transactions;

public class TransactionsMockService : ITransactionsService
{
    private readonly IServicesMockData _data;

    public TransactionsMockService(IServicesMockData data)
    {
        _data = data;
    }

    public Task<ObservableCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        Guard.Against.Null(accountAddress, nameof(accountAddress));
        Guard.Against.NegativeOrZero(lastTransactionsCount, nameof(lastTransactionsCount));

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
