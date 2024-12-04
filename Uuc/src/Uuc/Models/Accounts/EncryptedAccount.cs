namespace Uuc.Models.Accounts;

public class EncryptedAccount : BaseAccount
{
	public byte[] EncryptedKey { get; set; } = null!;
}
