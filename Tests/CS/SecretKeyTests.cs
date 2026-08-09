using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Uccs.Net;
using Uccs.Nexus;
using Xunit;

namespace Uccs.Tests;

public class SecretKeyTests
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


		Assert.True(k.PuplicKey == PublicKey.Parse(k.PuplicKey.ToString()));
		Assert.True(Cryptography.Mcv.Verify(k.PuplicKey, h, s));
		Assert.False(Cryptography.Mcv.Verify(kk.PuplicKey, h, s));
					
		var v = new Vault(Zone.Test, new VaultSettings{}, new Flow());

		string p = "password";
		
		var w = v.CreateWallet("123", p, 1);
		
		var raw = w.ToRaw();

		w.Lock();
		w.Unlock(p);

		w.ToRaw();

		var w1 = v.CreateWallet("2", w.ToRaw());
		w1.Unlock(p);
		
		Assert.Equal(w.Keys, w1.Keys, EqualityComparer<WalletKey>.Create((a, b) =>	{
																						return Bytes.EqualityComparer.Equals(a.Key.Secret, b.Key.Secret) && a.Name == b.Name;
																					}));
		//Assert.True(k == AccountKey.Load(Cryptography.Normal, k.Save(Cryptography.Normal, "123"), "123"));
 	}
}
