namespace UC.Net.Node.MAUI.Services;

public sealed class AccountsService : IAccountsService
{
    public Task<ObservableCollection<Account>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ObservableCollection<Account>> GetLastAsync(int lastAccountsCount)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync([NotNull] Account account)
    {
        throw new NotImplementedException();
    }

    public Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        throw new NotImplementedException();
    }
}
