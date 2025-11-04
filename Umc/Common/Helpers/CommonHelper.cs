using System.Security.Cryptography;

namespace UC.Umc.Helpers;

public static class CommonHelper
{
	public static string GetTodayDate => GetFormattedDate(DateTime.UtcNow);
	public static string GetFormattedDate(DateTime date) => date.ToString("dd MMM yyyy HH:mm");

	public static int GetDaysLeft(DateTime date) =>
		date == default ? 0
		: (int)(new TimeSpan(date.Ticks - DateTime.Now.Ticks).TotalDays);

    public static NetworkAccess CheckConnectivity() => Connectivity.Current.NetworkAccess;

	// WBD
    public static string GenerateUniqueId(int length)
    {
        int sufficientBufferSizeInBytes = (length * 6 + 7) / 8;
        var buffer = new byte[sufficientBufferSizeInBytes];
        RandomNumberGenerator.Create().GetBytes(buffer);
        return Convert.ToBase64String(buffer)[..length];
    }

    public static async Task<byte[]> ReadFile(string path)
    {
		byte[] result = null;
        try
        {
            using (var stream = File.OpenWrite(path))
            using (var newStream = new MemoryStream())
            {
                await stream.CopyToAsync(newStream);
                result = newStream.ToArray();
            }
        }
        catch(Exception ex)
        {
            await ToastHelper.ShowMessageAsync(Properties.Additional_Strings.Error_LoadingFile);

			ThrowHelper.ThrowInvalidOperationException("PathToBytes: Loading error", ex);
        }
        return result;
    }

	public static async Task<string> GetPathToWalletAsync()
	{
		try
		{
			var result = await FilePicker.Default.PickAsync(new()
			{
				PickerTitle = Properties.Additional_Strings.WalletFile_Select
			});

			if (result == null)
			{
				throw new FileNotFoundException();
			}

			return result?.FullPath ?? string.Empty;
		}
		catch (Exception ex)
		{
            await ToastHelper.ShowDefaultErrorMessageAsync();

			ThrowHelper.ThrowInvalidOperationException("GetPathToWalletAsync: Loading error", ex);
		}

		return null;
	}

    /// <summary>
    /// Awaits <c>Task</c>. Exceptions are handled by <c>errorAction</c>
    /// </summary>
    public static async void AwaitTaskAsync(this Task task, Action<Exception> errorAction, bool configureAwait = true)
    {
        try
        {
            await task.ConfigureAwait(configureAwait);
        }
        catch (Exception ex)
        {
            errorAction?.Invoke(ex);
        }
    }
}