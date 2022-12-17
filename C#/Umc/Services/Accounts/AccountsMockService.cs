namespace UC.Umc.Services;

public class AccountsMockService : IAccountsService
{
    private readonly IServicesMockData _data;

    public AccountsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<ObservableCollection<AccountViewModel>> GetAllAsync()
    {
        var result = new ObservableCollection<AccountViewModel>(_data.Accounts);
        return Task.FromResult(result);
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
        Guard.IsNotNull(account, nameof(account));

        AccountViewModel accountForUpdate =
            _data.Accounts.FirstOrDefault(x =>
                string.Equals(x.Address, account.Address, StringComparison.InvariantCultureIgnoreCase));
        Guard.IsNotNull(accountForUpdate);
        UpdateAccount(accountForUpdate, account);

        return Task.CompletedTask;
    }

    private void UpdateAccount(AccountViewModel destination, AccountViewModel source)
    {
        destination.Balance = source.Balance;
        destination.Color = source.Color;
        destination.Name = source.Name;
        destination.HideOnDashboard = source.HideOnDashboard;
    }

    public Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        Guard.IsNotNullOrEmpty(address, nameof(address));

		var account = _data.Accounts.FirstOrDefault(x => string.Equals(x.Address, address, StringComparison.InvariantCultureIgnoreCase));

        Guard.IsNotNull(account);

        _data.Accounts.Remove(account);

        return Task.CompletedTask;
    }
}
