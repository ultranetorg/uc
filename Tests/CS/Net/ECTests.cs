using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public static class ECTests
{
	static void SequenceEqual(EC[] a, EC[] b) => Assert.True(a.SequenceEqual(b));

	[Fact]
	public static void Main()
	{
		var a	= new EC(Time.FromDays(10), 10);
		var b	= new EC(Time.FromDays(20), 20);
		var c	= new EC(Time.FromDays(30), 30);
		var cc	= new EC(Time.FromDays(30), 33);
		var d	= new EC(Time.FromDays(40), 40);
		var e	= new EC(Time.FromDays(50), 40);
					
		SequenceEqual(EC.Add([b, c], a),	[a, b, c]);
		SequenceEqual(EC.Add([b, c], [a]),	[a, b, c]);

		SequenceEqual(EC.Add([a, c], b),	[a, b, c]);
		SequenceEqual(EC.Add([a, c], [b]),	[a, b, c]);

		SequenceEqual(EC.Add([b, c], d),	[b, c, d]);
		SequenceEqual(EC.Add([b, c], [d]),	[b, c, d]);

		SequenceEqual(EC.Add([a, c], [b, d]), [a, b, c, d]);
		SequenceEqual(EC.Add([b, d], [a, c]), [a, b, c, d]);

		SequenceEqual(EC.Add([b, c], cc),	[b, new (c.Expiration, c.Amount + cc.Amount)]);
		SequenceEqual(EC.Add([b, c], [cc]),	[b, new (c.Expiration, c.Amount + cc.Amount)]);

		SequenceEqual(EC.Add([b, c], [a, d]), [a, b, c, d]);

		Assert.True(EC.Integrate([b, c, d], Time.FromDays(0)) == b.Amount + c.Amount + d.Amount);
		Assert.True(EC.Integrate([b, c, d], Time.FromDays(10)) == b.Amount + c.Amount + d.Amount);
		Assert.True(EC.Integrate([b, c, d], Time.FromDays(30)) == c.Amount + d.Amount);
		Assert.True(EC.Integrate([b, c, d], Time.FromDays(40)) == d.Amount);
		Assert.True(EC.Integrate([b, c, d], Time.FromDays(50)) == 0);


		SequenceEqual(EC.Subtract([a, b, c], 1, b.Expiration),					 [new (b.Expiration, b.Amount-1), c]);
		SequenceEqual(EC.Subtract([a, b, c], a.Amount + b.Amount, a.Expiration), [c]);
		SequenceEqual(EC.Subtract([a, b, c], 1, e.Expiration),					 []);


		SequenceEqual(EC.Take([a, b, c], a.Amount + b.Amount + c.Amount, a.Expiration),	[a, b, c]);
		SequenceEqual(EC.Take([a, b, c], a.Amount + b.Amount, a.Expiration),				[a, b]);
		SequenceEqual(EC.Take([a, b, c], b.Amount, b.Expiration),							[b]);
		SequenceEqual(EC.Take([a, b, c], b.Amount + c.Amount/2, b.Expiration),			[b, new (c.Expiration, c.Amount/2)]);
	}
}
