using Uccs.Net;
using Xunit;

namespace Uccs.Tests;

public static class ECTests
{
	static void SequenceEqual(ExpiredEC[] a, ExpiredEC[] b) => Assert.True(a.SequenceEqual(b));

	static ExpiredEC a	= new ExpiredEC(Time.FromDays(10), 10);
	static ExpiredEC b	= new ExpiredEC(Time.FromDays(20), 20);
	static ExpiredEC c	= new ExpiredEC(Time.FromDays(30), 30);
	static ExpiredEC cc = new ExpiredEC(Time.FromDays(30), 33);
	static ExpiredEC d	= new ExpiredEC(Time.FromDays(40), 40);
	static ExpiredEC e	= new ExpiredEC(Time.FromDays(50), 40);

	[Fact]
	public static void Main()
	{
		SequenceEqual(ExpiredEC.Add([b, c], a),	[a, b, c]);
		SequenceEqual(ExpiredEC.Add([b, c], [a]),	[a, b, c]);

		SequenceEqual(ExpiredEC.Add([a, c], b),	[a, b, c]);
		SequenceEqual(ExpiredEC.Add([a, c], [b]),	[a, b, c]);

		SequenceEqual(ExpiredEC.Add([b, c], d),	[b, c, d]);
		SequenceEqual(ExpiredEC.Add([b, c], [d]),	[b, c, d]);

		SequenceEqual(ExpiredEC.Add([a, c], [b, d]), [a, b, c, d]);
		SequenceEqual(ExpiredEC.Add([b, d], [a, c]), [a, b, c, d]);

		SequenceEqual(ExpiredEC.Add([b, c], cc),	[b, new (c.Expiration, c.Amount + cc.Amount)]);
		SequenceEqual(ExpiredEC.Add([b, c], [cc]),	[b, new (c.Expiration, c.Amount + cc.Amount)]);

		SequenceEqual(ExpiredEC.Add([b, c], [a, d]), [a, b, c, d]);

		Assert.True(ExpiredEC.Integrate([b, c, d], Time.FromDays(0)) == b.Amount + c.Amount + d.Amount);
		Assert.True(ExpiredEC.Integrate([b, c, d], Time.FromDays(10)) == b.Amount + c.Amount + d.Amount);
		Assert.True(ExpiredEC.Integrate([b, c, d], Time.FromDays(30)) == c.Amount + d.Amount);
		Assert.True(ExpiredEC.Integrate([b, c, d], Time.FromDays(40)) == d.Amount);
		Assert.True(ExpiredEC.Integrate([b, c, d], Time.FromDays(50)) == 0);


		SequenceEqual(ExpiredEC.Subtract([a, b, c], 1, b.Expiration),					 [new (b.Expiration, b.Amount-1), c]);
		SequenceEqual(ExpiredEC.Subtract([a, b, c], a.Amount + b.Amount, a.Expiration), [c]);
		SequenceEqual(ExpiredEC.Subtract([a, b, c], 1, e.Expiration),					 []);


		SequenceEqual(ExpiredEC.Take([a, b, c], a.Amount + b.Amount + c.Amount, a.Expiration),	[a, b, c]);
		SequenceEqual(ExpiredEC.Take([a, b, c], a.Amount + b.Amount, a.Expiration),				[a, b]);
		SequenceEqual(ExpiredEC.Take([a, b, c], b.Amount, b.Expiration),							[b]);
		SequenceEqual(ExpiredEC.Take([a, b, c], b.Amount + c.Amount/2, b.Expiration),			[b, new (c.Expiration, c.Amount/2)]);
	}

	[Fact]
	public static void Move()
	{
		var x = new ExpiredEC[] {a, b, c};
		var y = new ExpiredEC[0];
		
		ExpiredEC.Move(ref x, ref y, a.Amount, a.Expiration);

		SequenceEqual(x, [b, c]);
		SequenceEqual(y, [a]);


		x = [a, b, c];
		y = [];
		
		ExpiredEC.Move(ref x, ref y, b.Amount, b.Expiration);

		SequenceEqual(x, [c]);
		SequenceEqual(y, [b]);


		x = [a, b, c];
		y = [];
		
		ExpiredEC.Move(ref x, ref y, 1, b.Expiration);

		SequenceEqual(x, [b.Add(-1), c]);
		SequenceEqual(y, [new ExpiredEC(b.Expiration, 1)]);

		
		x = [a, c];
		y = [b, d];
		
		ExpiredEC.Move(ref x, ref y, a.Amount + c.Amount, a.Expiration);

		SequenceEqual(x, []);
		SequenceEqual(y, [a, b, c, d]);
	}
}
