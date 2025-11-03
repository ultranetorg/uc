namespace UC.Umc.Services;

public class AccountsMockService : IAccountsService
{
    private readonly IServicesMockData _service;

    public AccountsMockService(IServicesMockData mockServiceData)
    {
        _service = mockServiceData;
    }

    public List<AccountViewModel> ListAllAccounts() => new(_service.Accounts);

    public async Task<List<AccountViewModel>> ListAccountsAsync(string filter = null, bool addAllOptions = false)
    {
		var accounts = _service.Accounts;
		if (addAllOptions)
		{
			accounts = accounts.Prepend(DefaultDataMock.AllAccountOption).ToList();
		}
		if (!string.IsNullOrEmpty(filter))
		{
			accounts = accounts.Where(x =>
				x.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase) ||
				x.Address.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
			.ToList();
		}
        return await Task.FromResult(accounts.ToList());
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

    public async Task UpdateAsync([NotNull] AccountViewModel account)
    {
        Guard.IsNotNull(account, nameof(account));

        AccountViewModel accountForUpdate =
            _service.Accounts.FirstOrDefault(x =>
                string.Equals(x.Address, account.Address, StringComparison.InvariantCultureIgnoreCase));
        Guard.IsNotNull(accountForUpdate);
        UpdateAccount(accountForUpdate, account);

        await Task.Delay(10);
    }

    private void UpdateAccount(AccountViewModel destination, AccountViewModel source)
    {
        destination.Balance = source.Balance;
        destination.Color = source.Color;
        destination.Name = source.Name;
        destination.HideOnDashboard = source.HideOnDashboard;
    }

    public async Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        Guard.IsNotNullOrEmpty(address, nameof(address));

		var account = _service.Accounts.FirstOrDefault(x => string.Equals(x.Address, address, StringComparison.InvariantCultureIgnoreCase));

        Guard.IsNotNull(account);

        _service.Accounts.Remove(account);

        await Task.Delay(10);
    }
}
