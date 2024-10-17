using System.Diagnostics.CodeAnalysis;
using UC.Umc.Models;

namespace UC.Umc.Services.Accounts;

public class AccountsMockService(IServicesMockData mockServiceData) : IAccountsService
{
	public List<AccountModel> ListAllAccounts() => new(mockServiceData.Accounts);

	public Task<List<AccountModel>> ListAccountsAsync(string filter = null, bool addAllOptions = false)
	{
		var accounts = mockServiceData.Accounts;
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
		return Task.FromResult(accounts.ToList());
	}

	public Task<string> GetPrivateKeyAsync(string address)
	{
		throw new NotImplementedException();
	}

	public Task CreateAccountAsync(AccountModel account)
	{
		throw new NotImplementedException();
	}

	public Task RestoreAccountAsync(AccountModel account)
	{
		throw new NotImplementedException();
	}

	public Task UpdateAsync(AccountModel account)
	{
		AccountModel accountForUpdate =
			mockServiceData.Accounts.FirstOrDefault(x =>
				string.Equals(x.Address, account.Address, StringComparison.InvariantCultureIgnoreCase));
		Guard.IsNotNull(accountForUpdate);
		UpdateAccount(accountForUpdate, account);

		return Task.CompletedTask;
	}

	private void UpdateAccount(AccountModel destination, AccountModel source)
	{
		destination.Balance = source.Balance;
		destination.Color = source.Color;
		destination.Name = source.Name;
		destination.HideOnDashboard = source.HideOnDashboard;
	}

	public Task DeleteByAddressAsync([NotEmpty] string address)
	{
		Guard.IsNotEmpty(address, nameof(address));

		var account = mockServiceData.Accounts.FirstOrDefault(x => string.Equals(x.Address, address, StringComparison.InvariantCultureIgnoreCase));

		Guard.IsNotNull(account);

		mockServiceData.Accounts.Remove(account);

		return Task.CompletedTask;
	}
}
