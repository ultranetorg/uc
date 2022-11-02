namespace UC.Net.Node.MAUI.Services;

public interface IAccountsService
{
    Task<ObservableCollection<AccountViewModel>> GetAllAsync();

    Task<int> GetCountAsync();

    Task<ObservableCollection<AccountViewModel>> GetLastAsync(int lastAccountsCount);

    Task UpdateAsync([NotNull] AccountViewModel account);

    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);
}
