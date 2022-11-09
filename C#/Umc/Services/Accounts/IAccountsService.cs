namespace UC.Umc.Services;

public interface IAccountsService
{
    Task<ObservableCollection<AccountViewModel>> GetAllAsync();

    Task UpdateAsync([NotNull] AccountViewModel account);

    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);
}
