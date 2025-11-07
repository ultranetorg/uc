namespace UC.Umc.Services;

public class TransactionsMockService : ITransactionsService
{
    private readonly IServicesMockData _service;

    public TransactionsMockService(IServicesMockData data)
    {
        _service = data;
    }

    public async Task<CustomCollection<TransactionViewModel>> ListTransactionsAsync(string accountAddress, string search, int count)
    {
		var transactions = _service.Transactions;

		if (!string.IsNullOrEmpty(accountAddress))
		{
			transactions = transactions.Where(x => string.Equals(x.Account.Address, accountAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
		}
		if (!string.IsNullOrEmpty(search))
		{
			transactions = transactions.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
		}

        return await Task.FromResult(new CustomCollection<TransactionViewModel>(
			transactions.OrderByDescending(x => x.Date).Take(count > 1 ? count : SizeConstants.SizePerPageMed)));
    }
}
