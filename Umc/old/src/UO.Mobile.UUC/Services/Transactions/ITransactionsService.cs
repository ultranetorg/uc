using UO.Mobile.UUC.Models.Transactions;

namespace UO.Mobile.UUC.Services.Transactions;

public interface ITransactionsService
{
    Task<ObservableCollection<Transaction>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount);

    Task<ObservableCollection<Transaction>> GetLastAsync(int lastTransactionsCount);
}
