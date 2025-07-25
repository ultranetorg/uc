﻿using System.Text.Json;
using Uccs.Net;
using Uccs.Rdn;
using Xunit;


namespace Uccs.Tests;

public static class UraTests
{
	static byte[] RandomBytes(int n)
	{
		var b = new byte[n];
		Cryptography.Random.NextBytes(b);
		return b;
	}

 		[Fact]
 		public static void UAddresses()
 		{
		void check(string a)
		{
			Assert.True(Unea.Parse(a).ToString() == a);
		}

		check($"n");
		check($"n/e");
		check($"/e");
		check($"/e/x");
		check($"s:n");
		check($"s:n/e");
		check($"s:nb.nb/e");
// 			check($"s:z:d");
// 			check($"s:z:d/r");
// 			check($"s:z:s.d/r");
 		}

 		[Fact]
 		public static void Resource()
 		{
 			var s = new List<Ura>(){Ura.Parse($"{Ura.Scheme}:/a/r"),
								Ura.Parse($"{Ura.Scheme}:/aa/rr"),
								Ura.Parse($"{Ura.Scheme}:net/aaa/rrr")};

		Assert.True(s.Count(i => i == Ura.Parse($"{Ura.Scheme}:/a/r")) == 1);
		Assert.True(s.Count(i => i == Ura.Parse($"{Ura.Scheme}:/aa/rr")) == 1);
		Assert.True(s.Count(i => i == Ura.Parse($"{Ura.Scheme}:net/aaa/rrr")) == 1);

		Assert.DoesNotContain(Ura.Parse("ura:absent/aaa/rrr"), s);
 		}

 		[Fact]
 		public static void Package()
 		{
 			var p = new HashSet<AprvAddress>(){AprvAddress.Parse($"{Ura.Scheme}:/a/p/r/v")};
		Assert.Contains(AprvAddress.Parse($"{Ura.Scheme}:/a/p/r/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Ura.Scheme}:/a/p/r/v-"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Ura.Scheme}:/a/p/r-/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Ura.Scheme}:/a/p-/r/v"), p);
		Assert.DoesNotContain(AprvAddress.Parse($"{Ura.Scheme}:/a-/p/r/v"), p);
 		}

	[Fact]
	public static void Release()
	{
		var a = new Urrh { Hash = RandomBytes(32) };
		var ac = new Urrh{ Hash = a.Hash.ToArray() };
		var b = new Urrh { Hash = RandomBytes(32) };
		 
		var x = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = RandomBytes(65) };
		var xc = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = x.Signature.ToArray() };
		var y = new Urrsd { Resource = Ura.Parse($"{Ura.Scheme}:/a/p"), Signature = RandomBytes(65) };

		Assert.True(a == ac && a != b &&
					x == xc && x != y &&
					a != x && ac != xc);

		var l = new List<Urr> {a, x};

		Assert.Contains(a, l);
		Assert.Contains(ac, l);
		Assert.DoesNotContain(b, l);

		Assert.Contains(x, l);
		Assert.Contains(x, l);
		Assert.DoesNotContain(y, l);

		Assert.True(a == Urr.Parse(a.ToString()));



		Assert.True(a == JsonSerializer.Deserialize<Urr>(JsonSerializer.Serialize((Urr)a, RdnApiClient.CreateOptions()), RdnApiClient.CreateOptions()));
		Assert.True(x == JsonSerializer.Deserialize<Urr>(JsonSerializer.Serialize((Urr)x, RdnApiClient.CreateOptions()), RdnApiClient.CreateOptions()));
		
		Assert.True(x == JsonSerializer.Deserialize<A>(JsonSerializer.Serialize(new A{RR = x}, RdnApiClient.CreateOptions()), RdnApiClient.CreateOptions()).RR);
	}

	class A
	{
		public Urr RR {get;set; }
	}

	//[Theory]
	//[MemberData(nameof(FromWei_TestData))]
	//public static void FromWei(BigInteger wei, Coin expected)
	//{
	//	Coin actual = Coin.FromWei(wei);
	//	Assert.Equal(expected, actual);
	//}

	//public static IEnumerable<object[]> FromWei_TestData()
	//{
	//	yield return new object[] { 1_002, new Coin() };
	//	yield return new object[] { 1_002, new Coin() };
	//	yield return new object[] { 1_002, new Coin() };
	//}

	//[Fact]
	//public static void Ctor_Int()
	//{
	//}

	//[Fact]
	//public static void Ctor_Decimal()
	//{
	//}

	//[Fact]
	//public static void Ctor_Double()
	//{
	//}
}
