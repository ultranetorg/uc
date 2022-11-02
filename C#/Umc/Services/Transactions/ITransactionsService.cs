namespace UC.Umc.Services;

public interface ITransactionsService
{
    Task<CustomCollection<TransactionViewModel>> GetLastForAccountAsync(string accountAddress, int lastTransactionsCount);

    Task<CustomCollection<TransactionViewModel>> GetLastAsync(int lastTransactionsCount);
}
