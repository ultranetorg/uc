namespace UC.Net.Node.MAUI.Services;

public class AccountsMockService : IAccountsService
{
    private readonly IServicesMockData _data;

    public AccountsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<ObservableCollection<Account>> GetAllAsync()
    {
        ObservableCollection<Account> result = new (_data.Accounts);
        return Task.FromResult(result);
    }

    public Task<int> GetCountAsync()
    {
        return Task.FromResult(_data.Accounts.Count);
    }

    public Task<ObservableCollection<Account>> GetLastAsync(int lastAccountsCount)
    {
        Guard.IsGreaterThan(lastAccountsCount, 0, nameof(lastAccountsCount));

        IEnumerable<Account> lastAccounts = _data.Accounts.Take(lastAccountsCount);
        ObservableCollection<Account> result = new(lastAccounts);
        return Task.FromResult(result);
    }

    public Task UpdateAsync([NotNull] Account account)
    {
        Guard.IsNotNull(account, nameof(account));

        Account accountForUpdate =
            _data.Accounts.FirstOrDefault(x =>
                string.Equals(x.Address, account.Address, StringComparison.InvariantCultureIgnoreCase));
        Guard.IsNotNull(accountForUpdate);
        UpdateAccount(accountForUpdate, account);

        return Task.CompletedTask;
    }

    private void UpdateAccount(Account destination, Account source)
    {
        destination.Balance = source.Balance;
        destination.Color = source.Color;
        destination.Name = source.Name;
        destination.ShowOnDashboard = source.ShowOnDashboard;
    }

    public Task DeleteByAddressAsync([NotEmpty, NotNull] string address)
    {
        Guard.IsNotNullOrEmpty(address, nameof(address));

        _data.Accounts = _data.Accounts
            .Where(x => !string.Equals(x.Address, address, StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        return Task.CompletedTask;
    }
}
