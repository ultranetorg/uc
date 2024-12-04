using System.Text.Json;
using CommunityToolkit.Diagnostics;
using Uccs.Net;
using Uuc.Common.Constants;
using Uuc.Models;
using Uuc.Models.Accounts;
using Account = Uuc.Models.Accounts.Account;

namespace Uuc.Services;

internal class AccountsService(IPasswordService passwordService, Cryptography cryptography) : IAccountsService
{
	public async Task<bool> AnyByName(string name)
	{
		Guard.IsNotEmpty(name);

		IList<EncryptedAccount>? encryptedAccounts = await LoadAllAsync();
		return encryptedAccounts?.Any(x => x.Name == name) ?? false;
	}

	public async Task CreateAsync(string name)
	{
		Guard.IsNotEmpty(name);

		AccountKey accountKey = AccountKey.Create();
		byte[] encryptedKey = EncryptAccountKey(accountKey);

		EncryptedAccount encryptedAccount = new()
		{
			Name = name,
			EncryptedKey = encryptedKey
		};
		IList<EncryptedAccount> encryptedAccounts = await LoadAllAsync() ?? new List<EncryptedAccount>();
		encryptedAccounts.Add(encryptedAccount);

		await SaveAllAsync(encryptedAccounts);
	}

	public async Task<IList<Account>?> ListAllAsync(CancellationToken cancellationToken = default)
	{
		IList<EncryptedAccount>? encryptedAccounts = await LoadAllAsync();
		return encryptedAccounts != null ? ToDecryptAccounts(encryptedAccounts) : null;
	}

	private byte[] EncryptAccountKey(AccountKey accountKey) => cryptography.Encrypt(accountKey, passwordService.Password);

	private IList<Account> ToDecryptAccounts(IList<EncryptedAccount> encryptedAccounts)
	{
		return encryptedAccounts.Select(x => new Account
		{
			Name = x.Name,
			Balances = x.Balances,
			AccountKey = cryptography.Decrypt(x.EncryptedKey, passwordService.Password)
		}).ToList();
	}

	private async Task<IList<EncryptedAccount>?> LoadAllAsync()
	{
		string? json = await SecureStorage.GetAsync(SecureStorageKeys.Accounts);
		return json != null ? JsonSerializer.Deserialize<IList<EncryptedAccount>>(json) : null;
	}

	private Task SaveAllAsync(IList<EncryptedAccount> encryptedAccounts)
	{
		string json = JsonSerializer.Serialize(encryptedAccounts);
		return SecureStorage.SetAsync(SecureStorageKeys.Accounts, json);
	}
}
