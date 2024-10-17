using System.Diagnostics.CodeAnalysis;
using UC.Umc.Models;

namespace UC.Umc.Services.Accounts;

public interface IAccountsService
{
	List<AccountModel> ListAllAccounts();

	Task<List<AccountModel>> ListAccountsAsync(string filter = null, bool addAllOptions = false);

	Task<string> GetPrivateKeyAsync(string address);

	Task CreateAccountAsync(AccountModel account);

	Task RestoreAccountAsync(AccountModel account);

	// hide from dashboard?
	Task UpdateAsync(AccountModel account);

	// delete by which ID?
	Task DeleteByAddressAsync([NotEmpty] string address);

	// Backup / Send / Receive?
}
