using System.Security.Cryptography;
using System.Text;

namespace Uuc.Common.Helpers;

internal static class HashHelper
{
	public static string GetSha256(string value)
	{
		var sha256 = SHA256.Create();
		byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));

		var sb = new StringBuilder();
		foreach (byte b in hash)
		{
			sb.Append(b.ToString("x2"));
		}

		return sb.ToString();
	}
}
