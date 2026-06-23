using System.Text.Json;
using Uccs.Net;
using Uccs.Rdn;
using Xunit;

namespace Uccs.Tests;

public static class AddressTests
{
	static byte[] RandomBytes(int n)
	{
		var b = new byte[n];
		Cryptography.Random.NextBytes(b);
		return b;
	}

 	[Fact]
 	public static void SchemeNetQuery()
 	{
		string[] a = [null, "", Iccn.Root];

		foreach(var i in a )
			foreach(var j in a)
				Assert.True(Snq.NetsEqual(i, j));

		string[] b = [".rdnx", "x.rdn.y"];

		foreach(var i in a )
			foreach(var j in b)
				Assert.False(Snq.NetsEqual(i, j));

		Assert.True(Snq.NetsEqual("n.rdn", "n"));

		void check(string a) => Assert.True(Snq.Parse(a).ToString() == a);

		check($"n");
		check($"n/e");
		check($"/e");
		check($"/e/x");
		check($"s:n");
		check($"s:n/e");
		check($"s:sn.n/e");
	}

 	[Fact]
 	public static void Resource()
 	{
 		var s = new List<Ura>(){Ura.Parse($"{Iccp.Scheme}:/a/r"),
								Ura.Parse($"{Iccp.Scheme}:/aa/rr"),
								Ura.Parse($"{Iccp.Scheme}:net/aaa/rrr")};

		Assert.True(s.Count(i => i == Ura.Parse($"{Iccp.Scheme}:/a/r")) == 1);
		Assert.True(s.Count(i => i == Ura.Parse($"{Iccp.Scheme}:/aa/rr")) == 1);
		Assert.True(s.Count(i => i == Ura.Parse($"{Iccp.Scheme}:net/aaa/rrr")) == 1);

		Assert.DoesNotContain(Ura.Parse("ura:absent/aaa/rrr"), s);
 	}

 	[Fact]
 	public static void Package()
 	{
		var p = new HashSet<AprvAddress>(){AprvAddress.Parse($"{Iccp.Scheme}:/a/p/r/v")};
		Assert.Contains(AprvAddress.Parse($"{Iccp.Scheme}:/a/p/r/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Iccp.Scheme}:/a/p/r/v-"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Iccp.Scheme}:/a/p/r-/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Iccp.Scheme}:/a/p-/r/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Iccp.Scheme}:/a-/p/r/v"), p);
	}

	[Fact]
	public static void Release()
	{
		var a = new Rrrh {Hash = RandomBytes(32) };
		var ac = new Rrrh{Hash = a.Hash.ToArray() };
		var b = new Rrrh {Hash = RandomBytes(32) };
		 
		//var x = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = RandomBytes(65) };
		//var xc = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = x.Signature.ToArray() };
		//var y = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = RandomBytes(65) };

		//Assert.True(a == ac && a != b &&
		//			x == xc && x != y &&
		//			a != x && ac != xc);

		//var l = new List<Urr> {a, x};

		//Assert.Contains(a, l);
		//Assert.Contains(ac, l);
		//Assert.DoesNotContain(b, l);

		//Assert.Contains(x, l);
		//Assert.Contains(x, l);
		//Assert.DoesNotContain(y, l);

		Assert.True(a == Urr.Parse(a.ToString()));



		Assert.True(a == JsonSerializer.Deserialize<Urr>(JsonSerializer.Serialize((Urr)a, RdnJsonConfiguration.CreateOptions()), RdnJsonConfiguration.CreateOptions()));
		//Assert.True(x == JsonSerializer.Deserialize<Urr>(JsonSerializer.Serialize((Urr)x, RdnJsonConfiguration.CreateOptions()), RdnJsonConfiguration.CreateOptions()));
		//
		//Assert.True(x == JsonSerializer.Deserialize<A>(JsonSerializer.Serialize(new A{RR = x}, RdnJsonConfiguration.CreateOptions()), RdnJsonConfiguration.CreateOptions()).RR);
	}

	class A
	{
		public Urr RR {get;set; }
	}
}
