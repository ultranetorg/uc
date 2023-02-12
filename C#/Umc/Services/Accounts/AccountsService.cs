namespace UC.Umc.Services;

public sealed class AccountsService : IAccountsService
{
    public List<AccountViewModel> ListAllAccounts()
    {
        throw new NotImplementedException();
    }

    public Task<List<AccountViewModel>> ListAccountsAsync(string filter, bool addAllOptions = false)
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
