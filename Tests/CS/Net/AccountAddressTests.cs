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

			a = new AccountAddress(h, "o1zX");

			var t = a.ToString();
			Assert.True(t[0..4].SequenceEqual(a.Tag));

			var x = AccountAddress.Parse(t);

			Assert.True(x.Bytes.SequenceEqual(h));
			Assert.True(x.Tag == a.Tag);
		}
 	}
}
