using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;

namespace UC.Umc.Services;

public class AuthService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Allows User to use pin instead of Biometrics. Only supported on iOS and WinUI
    /// </summary>
    private static bool AllowAlternativeAuthentication => DeviceInfo.Current.Platform != DevicePlatform.iOS &&
                                                          DeviceInfo.Current.Platform != DevicePlatform.WinUI;

	public AuthService(ILogger<AuthService> logger)
	{
		_logger = logger;
	}
	
	// Performs pincode check
	public async Task<bool> LoginAsync(string pincode)
	{
		try
		{
			var pin = await UserSecureStore.GetDataAsync(CommonConstants.PINCODE_KEY);

			// ask to input pincode again (TBD)
            await UserSecureStore.SetUserDataAsync(CommonConstants.LAST_LOGIN, DateTime.Now.ToShortTimeString(), _logger);

			return !string.IsNullOrEmpty(pincode) && pincode == pin;
		}
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync Error: {Ex}", ex.Message);
			return false;
        }
	}
	
	// Creates pincode
	public async Task CreatePincodeAsync(string pincode)
	{
		try
		{
			await UserSecureStore.SetUserDataAsync(CommonConstants.PINCODE_KEY, pincode, _logger);

			// after a while we will ask to change pincode (TBD)
			await UserSecureStore.SetUserDataAsync(CommonConstants.PINCODE_SET, DateTime.Today.ToShortDateString(), _logger);

			await ToastHelper.ShowMessageAsync(Properties.Additional_Strings.Message_PinCreated);
		}
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePincodeAsync Error: {Ex}", ex.Message);
        }
	}

	public async Task<CheckBiometricsResult> CheckBiometricsAsync()
    {
        try
        {
#if DEBUG
			return CheckBiometricsResult.Authenticated;
#endif
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

    public async Task<bool> CheckIfPincodeIsSetAsync()
    {
        try
        {
			var pin = await UserSecureStore.GetDataAsync(CommonConstants.PINCODE_KEY);

			return !string.IsNullOrEmpty(pin) && pin.Length == 4;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckIfPincodeIsSetAsync Error: {Ex}", ex.Message);
            return false;
        }
    }

    private async Task<bool> IsBiometricAuthenticationActiveAsync()
    {
        try
        {
            return await CrossFingerprint.Current.IsAvailableAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsBiometricAuthenticationActiveAsync Error: {Ex}", ex.Message);
            return false;
        }
    }
}
