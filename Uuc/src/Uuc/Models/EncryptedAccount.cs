namespace Uuc.Models;

public class EncryptedAccount : BaseAccount
{
	public byte[] EncryptedKey { get; set; } = null!;
}
