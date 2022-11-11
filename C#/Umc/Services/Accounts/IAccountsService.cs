namespace UC.Umc.Services;

public interface IAccountsService
{
    Task<ObservableCollection<AccountViewModel>> GetAllAsync();

    Task<string> GetPrivateKeyAsync(string address);

	Task CreateAccountAsync(CreateAccountWorkflow workflow);

	Task RestoreAccountAsync(RestoreAccountWorkflow workflow);

	// hide from dashboard?
    Task UpdateAsync([NotNull] AccountViewModel account);

	// delete by which ID?
    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);

	// Backup / Send / Receive?
}
