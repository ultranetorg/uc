using Org.BouncyCastle.Security;
using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public class AccountKeyTests
{
 	[Fact]
 	public static void Main()
 	{
		var r = new SecureRandom();

		var h = new byte[32];
		r.NextBytes(h);

		var k = AccountKey.Create();
		var kk = AccountKey.Create();

		var s = Cryptography.Mcv.Sign(k, h);


		Assert.True(k.Address == AccountAddress.Parse(k.Address.ToString()));
		Assert.True(Cryptography.Mcv.Valid(s, h, k.Address));
		Assert.False(Cryptography.Mcv.Valid(s, h, kk.Address));
					
		string p = "password";
		Assert.True(Vault.Vault.Decrypt(Vault.Vault.Encrypt(h, p), p).SequenceEqual(h));
		Assert.Equal(Vault.Vault.Decrypt(Vault.Vault.Encrypt(h, p), p), h);
		//Assert.True(k == AccountKey.Load(Cryptography.Normal, k.Save(Cryptography.Normal, "123"), "123"));
 	}
}
