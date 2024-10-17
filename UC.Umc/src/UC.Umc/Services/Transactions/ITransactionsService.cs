using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Transactions;

public interface ITransactionsService
{
	Task<ObservableCollection<TransactionModel>> ListTransactionsAsync(string accountAddress, string search, int lastTransactionsCount);
}
