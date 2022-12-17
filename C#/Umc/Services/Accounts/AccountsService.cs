namespace UC.Umc.Services;

public sealed class AccountsService : IAccountsService
{
    public Task<ObservableCollection<AccountViewModel>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

	public Task<string> GetPrivateKeyAsync(string address)
	{
		throw new NotImplementedException();
	}

	public Task CreateAccountAsync(AccountViewModel account)
	{
		throw new NotImplementedException();
	}

	public Task RestoreAccountAsync(AccountViewModel account)
	{
		throw new NotImplementedException();
	}

    public Task UpdateAsync([NotNull] AccountViewModel account)
    {
        throw new NotImplementedException();
    }

    public Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        throw new NotImplementedException();
    }
}
