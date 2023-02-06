namespace UC.Umc.Services;

public class TransactionsMockService : ITransactionsService
{
    private readonly IServicesMockData _service;

    public TransactionsMockService(IServicesMockData data)
    {
        _service = data;
    }

    public Task<CustomCollection<TransactionViewModel>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount = 10)
    {
        Guard.IsNotNull(accountAddress, nameof(accountAddress));

        var lastTransactions = _service.Transactions
            .Where(x => string.Equals(x.Account.Address, accountAddress, StringComparison.InvariantCultureIgnoreCase))
            .Take(lastTransactionsCount > 10 ? lastTransactionsCount : SizeConstants.SizePerPageMin);
        return Task.FromResult(new CustomCollection<TransactionViewModel>(lastTransactions));
    }

    public Task<CustomCollection<TransactionViewModel>> GetLastAsync(int lastTransactionsCount = 10) =>
		Task.FromResult(new CustomCollection<TransactionViewModel>(_service.Transactions.Take(
			lastTransactionsCount > 10 ? lastTransactionsCount : SizeConstants.SizePerPageMin)));
}
