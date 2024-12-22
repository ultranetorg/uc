using Org.BouncyCastle.Security;
using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public class AccountKeyTests
{
 	[Fact]
 		public static void Main()
 		{
// 			var e = new EthECKey(File.ReadAllBytes("m:\\UO\\Testdata\\Net\\Fathers\\0x000038a7a3cb80ec769c632b7b3e43525547ecd1.sunwpk"), true);
// 
// 			var initaddr = Sha3Keccack.Current.CalculateHash(e.GetPubKeyNoPrefix());
// 			var initaddr2 = Cryptography.Hash(e.GetPubKeyNoPrefix());
// 			var Bytes = new byte[initaddr.Length - 12];
// 			Array.Copy(initaddr, 12, Bytes, 0, initaddr.Length - 12);
		


		var r = new SecureRandom();

		var h = new byte[32];
		r.NextBytes(h);

		var k = AccountKey.Create();
		var kk = AccountKey.Create();

		var s = Cryptography.Normal.Sign(k, h);


		Assert.True(k == new AccountKey(k.GetPrivateKeyAsBytes()));
		Assert.True(k == AccountKey.Parse(k.GetPrivateKey()));
		Assert.True(k == AccountAddress.Parse(k.ToString()));
		Assert.True(Cryptography.Normal.Valid(s, h, k));
		Assert.False(Cryptography.Normal.Valid(s, h, kk));
					
		Assert.True(k == AccountKey.Load(Cryptography.Normal, k.Save(Cryptography.Normal, "123"), "123"));
 		}
}
