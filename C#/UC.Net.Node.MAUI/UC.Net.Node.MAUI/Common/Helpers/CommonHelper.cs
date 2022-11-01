using System.Security.Cryptography;

namespace UC.Net.Node.MAUI.Helpers;

public static class CommonHelper
{
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
}