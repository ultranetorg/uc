using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Accounts;

public sealed class AccountsService : IAccountsService
{
    public Task<ObservableCollection<Account>> GetAllAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<int> GetCountAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<ObservableCollection<Account>> GetLastAsync(int lastAccountsCount)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateAsync([NotNull] Account account)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        throw new System.NotImplementedException();
    }
}
