using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Accounts;

public interface IAccountsService
{
    Task<ObservableCollection<Account>> GetAllAsync();

    Task<int> GetCountAsync();

    Task<ObservableCollection<Account>> GetLastAsync(int lastAccountsCount);

    Task UpdateAsync([NotNull] Account account);

    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);
}
