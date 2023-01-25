using System.Security.Cryptography;

namespace UC.Umc.Helpers;

public static class CommonHelper
{
	public static int GetDaysLeft(DateTime date) =>
		date == default ? 0
		: (int)(new TimeSpan(date.Ticks - DateTime.Now.Ticks).TotalDays);

	// WBD
    public static string GenerateUniqueID(int length)
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
            await ToastHelper.ShowMessageAsync("Loading error");
			ThrowHelper.ThrowInvalidOperationException("PathToBytes: Loading error", ex);
        }
        return result;
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