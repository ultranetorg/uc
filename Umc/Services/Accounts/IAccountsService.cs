﻿namespace UC.Umc.Services;

public interface IAccountsService
{
    List<AccountViewModel> ListAllAccounts();

    Task<List<AccountViewModel>> ListAccountsAsync(string filter = null, bool addAllOptions = false);

    Task<string> GetPrivateKeyAsync(string address);

	Task CreateAccountAsync(AccountViewModel account);

	Task RestoreAccountAsync(AccountViewModel account);

	// hide from dashboard?
    Task UpdateAsync([NotNull] AccountViewModel account);

	// delete by which ID?
    Task DeleteByAddressAsync([NotNull, NotEmpty] string address);

	// Backup / Send / Receive?
}
