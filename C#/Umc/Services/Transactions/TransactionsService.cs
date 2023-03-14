namespace UC.Umc.Services;

public class TransactionsService : ITransactionsService
{
    public Task<CustomCollection<TransactionViewModel>> ListTransactionsAsync(string accountAddress, string search, int count)
    {
        throw new NotImplementedException();
    }
}
