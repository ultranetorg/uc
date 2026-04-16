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

			var x = new AccountAddress(h);

			Assert.True(AccountAddress.Parse(x.ToString()).Bytes.SequenceEqual(h));

			r.NextBytes(h);

			var tag = string.Concat(Enumerable.Range(0, 1000).Select(i => (char)(byte)r.Next()).Where(Bech32.Alphanumeric.Contains).Take(1 + r.Next() % (Bech32.MaxTagLength - 1)));
			x = new AccountAddress(h, tag);

			var t = x.ToString();
			Assert.StartsWith(tag, t);

			var y = AccountAddress.Parse(t);

			Assert.True(y.Bytes.SequenceEqual(h));
			Assert.True(y.Tag == x.Tag);
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

	[Fact]
 	public static void FormatAndAlgorithm()
 	{
		var r = new Random();
		var h = new byte[32];

		r.NextBytes(h);

		var items = new List<AccountAddress>();

		foreach(var f in Enum.GetValues<AddressFormat>())
		{
			foreach(var a in Enum.GetValues<Algorithm>())
			{
				items.Add(new AccountAddress(f, a, h, null));
			}
		}

		Assert.True(items.Distinct().Count() == items.Count);
 	}
}
