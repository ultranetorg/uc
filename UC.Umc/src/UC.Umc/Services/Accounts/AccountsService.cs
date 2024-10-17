using System.Diagnostics.CodeAnalysis;
using UC.Umc.Models;

namespace UC.Umc.Services.Accounts;

public sealed class AccountsService : IAccountsService
{
	public List<AccountModel> ListAllAccounts()
	{
		throw new NotImplementedException();
	}

	public Task<List<AccountModel>> ListAccountsAsync(string filter, bool addAllOptions = false)
	{
		throw new NotImplementedException();
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
		throw new NotImplementedException();
	}

	public Task DeleteByAddressAsync([NotEmpty] string address)
	{
		throw new NotImplementedException();
	}
}
