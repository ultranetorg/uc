using System.Text;
using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public class AccountAddressTests
{
 	[Fact]
 	public static void Main()
 	{
		var r = new Random();
		var h = new byte[32];

		for(int i = 0; i < 100; i++)
		{
			r.NextBytes(h);

			var a = new AccountAddress(h);

			Assert.True(AccountAddress.Parse(a.ToString()).Bytes.SequenceEqual(h));

			r.NextBytes(h);

			var tag = string.Concat(Enumerable.Range(0, 1000).Select(i => (char)r.Next()).Where(Bech32.Alphanumeric.Contains).Take(r.Next() % Bech32.MaxTagLength));
			a = new AccountAddress(h, tag);

			var t = a.ToString();
			//Assert.True(tag.SequenceEqual(a.Tag));

			var x = AccountAddress.Parse(t);

			Assert.True(x.Bytes.SequenceEqual(h));
			Assert.True(x.Tag == a.Tag);
		}
 	}
}
