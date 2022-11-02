namespace UC.Umc.Services;

public class TransactionsMockService : ITransactionsService
{
    private readonly IServicesMockData _data;

    public TransactionsMockService(IServicesMockData data)
    {
        _data = data;
    }

    public Task<CustomCollection<TransactionViewModel>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount)
    {
        Guard.IsNotNull(accountAddress, nameof(accountAddress));
        Guard.IsGreaterThan(lastTransactionsCount, 0, nameof(lastTransactionsCount));

        IEnumerable<TransactionViewModel> lastTransactions = _data.Transactions
            .Where(x => string.Equals(x.Account.Address, accountAddress, StringComparison.InvariantCultureIgnoreCase))
            .Take(lastTransactionsCount);
        CustomCollection<TransactionViewModel> result = new(lastTransactions);
        return Task.FromResult(result);
    }

    public Task<CustomCollection<TransactionViewModel>> GetLastAsync(int lastTransactionsCount)
    {
        IEnumerable<TransactionViewModel> lastTransactions = _data.Transactions.Take(lastTransactionsCount);
        CustomCollection<TransactionViewModel> result = new(lastTransactions);
        return Task.FromResult(result);
    }
}
