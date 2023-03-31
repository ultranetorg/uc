using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace UC.Umc.Services;

public class AuthService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Allows User to use pin instead of Biometrics. Only supported on iOS and MacCatalyst
    /// </summary>
    private static bool AllowAlternativeAuthentication => DeviceInfo.Current.Platform == DevicePlatform.iOS ||
                                                          DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst;

	public AuthService(ILogger<AuthService> logger)
	{
		_logger = logger;
	}

	public async Task<bool> LoginAsync(string pincode)
	{
		try
		{
			// perform pincode check
			await Task.Delay(10);

			return !string.IsNullOrEmpty(pincode) && pincode == "1111";
		}
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync Error: {Ex}", ex.Message);
			return false;
        }
	}

    public async Task<CheckBiometricsResult> CheckBiometricsAsync()
    {
        try
        {
//#if DEBUG
//            return CheckBiometricsResult.Authenticated;
//#endif
            var biometricsAvailable = await IsBiometricAuthenticationActiveAsync();

            if (!biometricsAvailable)
            {
                return CheckBiometricsResult.Disabled;
            }

            var request =
                new AuthenticationRequestConfiguration("Sign in to Ultranet Mobile",
                    "Use your fingerprint to sign in to Ultranet Mobile.")
                {
                    AllowAlternativeAuthentication = AllowAlternativeAuthentication
                };

            var result = await CrossFingerprint.Current.AuthenticateAsync(request);
            return result.Authenticated
                ? CheckBiometricsResult.Authenticated
                : CheckBiometricsResult.NotAuthenticated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckBiometricsAsync Error: {Ex}", ex.Message);
            return CheckBiometricsResult.NotAuthenticated;
        }
    }

    private async Task<bool> IsBiometricAuthenticationActiveAsync()
    {
        try
        {
            return await CrossFingerprint.Current.IsAvailableAsync(AllowAlternativeAuthentication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsBiometricAuthenticationActiveAsync Error: {Ex}", ex.Message);
            return false;
        }
    }
}
