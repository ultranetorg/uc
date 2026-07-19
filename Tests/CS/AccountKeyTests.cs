using Org.BouncyCastle.Security;
using Uccs.Net;
using Uccs.Nexus;
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

		var k = SecretKey.Create();
		var kk = SecretKey.Create();

		var s = Cryptography.Mcv.Sign(k, h);


		Assert.True(k.Address == AccountAddress.Parse(k.Address.ToString()));
		Assert.True(Cryptography.Mcv.Verify(k.Address, h, s));
		Assert.False(Cryptography.Mcv.Verify(kk.Address, h, s));
					
		var v = new Vault(Zone.Test, new VaultSettings{}, new Flow());
		
		string p = "password";
		
		Assert.True(v.Decrypt(v.Encrypt(h, p), p).SequenceEqual(h));
		Assert.Equal(v.Decrypt(v.Encrypt(h, p), p), h);
		//Assert.True(k == AccountKey.Load(Cryptography.Normal, k.Save(Cryptography.Normal, "123"), "123"));
 	}
}
