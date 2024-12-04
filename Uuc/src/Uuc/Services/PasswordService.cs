using CommunityToolkit.Diagnostics;
using Uuc.Common.Constants;
using Uuc.Common.Helpers;

namespace Uuc.Services;

public class PasswordService : IPasswordService
{
	// IPasswordService
	public string? Password { get; set; }

	public async Task<bool> IsValidAsync(string password)
	{
		Guard.IsNotEmpty(password);

		string? storedHash = await SecureStorage.GetAsync(SecureStorageKeys.PasswordHash);
		if (string.IsNullOrEmpty(storedHash))
		{
			return false;
		}

		string passwordHash = HashHelper.GetSha256(password);
		return storedHash == passwordHash;
	}

	public Task SaveHashAsync(string password)
	{
		Guard.IsNotEmpty(password);

		Password = password;

		string passwordHash = HashHelper.GetSha256(Password);
		return SecureStorage.SetAsync(SecureStorageKeys.PasswordHash, passwordHash);
	}

	public async Task<bool> IsHashSavedAsync()
	{
		string? storedHash = await SecureStorage.GetAsync(SecureStorageKeys.PasswordHash);
		return !string.IsNullOrEmpty(storedHash);
	}
}
