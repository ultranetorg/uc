using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Uccs.Net;
using Uccs.Nexus;
using Xunit;

namespace Uccs.Tests;

public class SecretKeyTests
{
 	[Fact]
 	public static void General()
 	{
		var r = new SecureRandom();

		var h = new byte[32];
		r.NextBytes(h);

		var k = SecretKey.Create();
		var kk = SecretKey.Create();

		var s = Cryptography.Mcv.Sign(k, h, SigningFeatures.None);
		var sd = Cryptography.Mcv.Sign(k, h, SigningFeatures.Deterministic);


		Assert.True(k.Puplic == PublicKey.Parse(k.Puplic.ToString()));
		Assert.True(Cryptography.Mcv.Verify(k.Puplic, h, s));
		Assert.True(Cryptography.Mcv.Verify(k.Puplic, h, sd));
		Assert.False(Cryptography.Mcv.Verify(kk.Puplic, h, s));
					
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
																						return Bytes.EqualityComparer.Equals(a.Secret.Secret, b.Secret.Secret) && a.Alias == b.Alias;
																					}));
		//Assert.True(k == AccountKey.Load(Cryptography.Normal, k.Save(Cryptography.Normal, "123"), "123"));
 	}
}
