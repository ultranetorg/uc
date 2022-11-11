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

	public Task CreateAccountAsync(CreateAccountWorkflow workflow)
	{
		throw new NotImplementedException();
	}

	public Task RestoreAccountAsync(RestoreAccountWorkflow workflow)
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
