using System.Collections.ObjectModel;
using UC.Umc.Common.Constants;
using UC.Umc.Models;

namespace UC.Umc.Services.Transactions;

public class TransactionsMockService(IServicesMockData data) : ITransactionsService
{
	public Task<ObservableCollection<TransactionModel>> ListTransactionsAsync(string accountAddress, string search, int count)
	{
		var transactions = data.Transactions;

		if (!string.IsNullOrEmpty(accountAddress))
		{
			transactions = transactions.Where(x => string.Equals(x.Account.Address, accountAddress, StringComparison.InvariantCultureIgnoreCase)).ToList();
		}
		if (!string.IsNullOrEmpty(search))
		{
			transactions = transactions.Where(x => x.Name.Contains(search, StringComparison.InvariantCultureIgnoreCase)).ToList();
		}

		return Task.FromResult(new ObservableCollection<TransactionModel>(
			transactions.OrderByDescending(x => x.Date).Take(count > 1 ? count : SizeConstants.SizePerPageMed)));
	}
}
