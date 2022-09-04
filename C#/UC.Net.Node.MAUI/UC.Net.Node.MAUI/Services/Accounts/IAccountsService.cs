namespace UC.Net.Node.MAUI.Services;

public interface IAccountsService
{
    Task<ObservableCollection<Account>> GetAllAsync();

    Task<int> GetCountAsync();

    Task<ObservableCollection<Account>> GetLastAsync(int lastAccountsCount);

    Task UpdateAsync([NotNull] Account account);

    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);
}
