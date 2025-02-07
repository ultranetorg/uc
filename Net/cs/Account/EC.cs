
namespace Uccs.Net;

public struct EC : IBinarySerializable, IEquatable<EC>
{
	public Time Expiration;
	public long	Amount;

	public static long	Integrate(EC[] x, Time expiration) => x?.SkipWhile(i => i.Expiration < expiration).Sum(i => i.Amount) ?? 0;

	public EC()
	{
	}

	public EC(Time expiration, long amount)
	{
		Expiration = expiration;
		Amount = amount;
	}

	public override string ToString()
	{
		return $"{Amount}, {Expiration}";
	}

	public void Read(BinaryReader reader)
	{
		Expiration	= reader.Read<Time>();
		Amount		= reader.Read7BitEncodedInt64();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Expiration); 
		writer.Write7BitEncodedInt64(Amount);
	}

	public EC Add(long y)
	{
		return new EC(Expiration, Amount + y);
	}

	public static EC[] Add(EC[] x, EC[] y)
	{
		var r = x;

		foreach(var i in y)
		{
			r = Add(r, i);
		}

		return r;
	}

	public static EC[] Add(EC[] x, EC y)
	{
		if(y.Amount == 0)	return [..x];
		if(y.Amount < 0)	throw new ArgumentOutOfRangeException();

		var e = Array.FindIndex(x, j => j.Expiration == y.Expiration);

		if(e == -1)
		{
			var i = Array.FindIndex(x, j => j.Expiration > y.Expiration);

			return i == -1 ? [..x, y] : [..x[0..i], y, ..x[i..]];
		} 
		else
		{
			EC[] r = [..x];
			
			r[e] = new (r[e].Expiration, r[e].Amount + y.Amount);

			return r;
		}
	}

	public static EC[] Subtract(EC[] x, long y, Time time)
	{
		if(y == 0)	return [..x];
		if(y < 0)	throw new ArgumentOutOfRangeException();

		int n = 0;

		foreach(var i in x)
		{
			if(i.Expiration < time)
			{	
				n++;
				continue;
			}

			y -= i.Amount;

			if(y >= 0)
			{
				n++;
			}
			else
				break;
		}

		var r = x[n..];

		if(y < 0)
			r[0] = new (r[0].Expiration, -y);

		return r;
	}

	public static EC[] Take(EC[] x, long y, Time time)
	{
		if(y == 0)	return [..x];
		if(y < 0)	throw new ArgumentOutOfRangeException();

		int old = 0;
		int full = 0;
		long a = 0;

		foreach(var i in x)
		{
			if(i.Expiration < time)
			{	
				old++;
				continue;
			}

			if(a + i.Amount <= y)
			{
				full++;
				a += i.Amount;
			}
			else
			{	
				break;
			}
		}

		if(y - a > 0)
			return [..x[old..(old + full)], new (x[old + full].Expiration, y - a)];
		else
			return x[old..(old + full)];
	}

	public static void Move(ref EC[] from, ref EC[] to, long y, Time time)
	{
		if(y == 0)	return;
		if(y < 0)	throw new ArgumentOutOfRangeException();

		int old = 0;
		int full = 0;
		long a = 0;

		foreach(var i in from)
		{
			if(i.Expiration < time)
			{
				old++;
				continue;
			}

			if(a + i.Amount <= y)
			{
				to = Add(to, i);
				full++;
				a += i.Amount;
			}
			else
				break;
		}

		if(y - a > 0)
		{	
			to = Add(to, new EC(from[old+full].Expiration, y - a));
			from = [new (from[old+full].Expiration, from[old+full].Amount - (y - a)), ..from[(old+full + 1)..]];
		}
		else
			from = from[(old+full)..];

 		//var d = Take(from, x, expiration);
		//from = Subtract(from, x, expiration);
		//to = Add(to, d);
	}

	public override bool Equals(object obj)
	{
		return obj is EC eC && Equals(eC);
	}

	public bool Equals(EC a)
	{
		return Expiration == a.Expiration && Amount == a.Amount;
	}

	public static bool operator ==(EC left, EC right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EC left, EC right)
	{
		return !(left == right);
	}
}
