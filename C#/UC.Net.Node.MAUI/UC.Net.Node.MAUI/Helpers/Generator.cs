using System.Security.Cryptography;
namespace UC.Net.Node.MAUI;

public static class Generator
{
    public static string GenerateUniqueID(int length)
    {
        int sufficientBufferSizeInBytes = (length * 6 + 7) / 8;
        var buffer = new byte[sufficientBufferSizeInBytes];
        RandomNumberGenerator.Create().GetBytes(buffer);
        return Convert.ToBase64String(buffer)[..length];
    }
}
