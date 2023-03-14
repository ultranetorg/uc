namespace UC.Umc.Services;

public interface ITransactionsService
{
    Task<CustomCollection<TransactionViewModel>> ListTransactionsAsync(string accountAddress, string search, int lastTransactionsCount);
}
