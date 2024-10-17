using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Transactions;

public class TransactionsService : ITransactionsService
{
	public Task<ObservableCollection<TransactionModel>> ListTransactionsAsync(string accountAddress, string search, int count)
	{
		throw new NotImplementedException();
	}
}
