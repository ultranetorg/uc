namespace UC.Umc.Common.Helpers;

/// <summary>
///  Gets and Sets User Data in Secure Storage
/// </summary>
internal static class UserSecureStore
{
    internal static async Task SetUserDataAsync(string key, string value, ILogger logger)
    {
        Guard.IsNotNullOrEmpty(value);

        try
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
        catch (Exception ex)
        {
            /*
             * Docs state it's best to remove the setting if an error is thrown
             * Docs: https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?tabs=android#use-secure-storage
             */
            RemoveData(key);
            logger.LogError(ex, "SetUserDataAsync Exception: {Ex}", ex.Message);
            ThrowHelper.ThrowInvalidOperationException("Failed to Save Token to SecureStorage", ex);
        }
    }

    internal static async Task<string> GetDataAsync(string key)
    {
		string token;

        try
        {
            token = await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
			/*
             * Docs state it's best to remove the setting if an error is thrown
             * Docs: https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?tabs=android#use-secure-storage
             */
			RemoveData(key);
            throw;
        }

        return token;
    }

    internal static void RemoveData(string key)
    {
        SecureStorage.Default.Remove(key);
    }
}
