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

			var tag = string.Concat(Enumerable.Range(0, 1000).Select(i => (char)(byte)r.Next()).Where(Bech32.Alphanumeric.Contains).Take(1 + r.Next() % (Bech32.MaxTagLength - 1)));
			a = new AccountAddress(h, tag);

			var t = a.ToString();
			Assert.StartsWith(tag, t);

			var x = AccountAddress.Parse(t);

			Assert.True(x.Bytes.SequenceEqual(h));
			Assert.True(x.Tag == a.Tag);
		}
 	}

	[Fact]
 	public static void ReadWrite()
 	{
		var r = new Random();
		var h = new byte[32];

		for(int i = 0; i < 100; i++)
		{
			r.NextBytes(h);

			var tag = string.Concat(Enumerable.Range(0, 1000).Select(i => (char)(byte)r.Next()).Where(Bech32.Alphanumeric.Contains).Take(1 + r.Next() % (Bech32.MaxTagLength - 1)));
			var x = new AccountAddress(h, tag);

			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			x.Write(w);
			
			var y = new AccountAddress();

			s = new MemoryStream(s.ToArray());
			y.Read(new BinaryReader(s));

			Assert.True(x == y);
			Assert.True(x.Tag == y.Tag);
		}
 	}
}
