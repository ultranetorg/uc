namespace UC.Umc.Common.Helpers;

/// <summary>
///  Gets and Sets User Token in Secure Storage
/// </summary>
internal static class UserSecureStore
{
    private const string TOKEN_KEY = "bearer_token";

    internal static async Task SetUserDataAsync(string token, ILogger logger)
    {
        Guard.IsNotNullOrEmpty(token);

        try
        {
            await SecureStorage.Default.SetAsync(TOKEN_KEY, token);
        }
        catch (Exception ex)
        {
            /*
             * Docs state it's best to remove the setting if an error is thrown
             * Docs: https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?tabs=android#use-secure-storage
             */
            RemoveToken();
            logger.LogError(ex, "SetUserDataAsync Exception: {Ex}", ex.Message);
            ThrowHelper.ThrowInvalidOperationException("Failed to Save Token to SecureStorage", ex);
        }
    }

    internal static async Task<string> GetTokenAsync()
    {
        var token = string.Empty;

        try
        {
            token = await SecureStorage.Default.GetAsync(TOKEN_KEY);
        }
        catch
        {
            /*
             * Docs state it's best to remove the setting if an error is thrown
             * Docs: https://docs.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage?tabs=android#use-secure-storage
             */
            RemoveToken();
            throw;
        }

        return token;
    }

    internal static void RemoveToken()
    {
        SecureStorage.Default.Remove(TOKEN_KEY);
    }
}
