namespace Uccs.Net;

public struct EC : IBinarySerializable
{
	public Time Expiration;
	public long	Amount;

	public EC()
	{
	}

	public EC(Time expiration, long amount)
	{
		Expiration = expiration;
		Amount = amount;
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

	public static EC[] Subtract(EC[] x, long y, Time expiration)
	{
		if(y == 0)	return [..x];
		if(y < 0)	throw new ArgumentOutOfRangeException();

		int n = 0;

		foreach(var i in x)
		{
			if(i.Expiration < expiration)
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
			{
				break;
			}
		}

		var r = x[n..];

		if(y < 0)
			r[0] = new (r[0].Expiration, -y);

		return r;
	}

	public static EC[] Difference(EC[] x, long y, Time expiration)
	{
		if(y == 0)	return [..x];
		if(y < 0)	throw new ArgumentOutOfRangeException();

		int fulls = 0;
		long a = 0;

		foreach(var i in x.Where(i => i.Expiration >= expiration))
		{
			if(a + i.Amount <= y)
			{
				fulls++;
				a += i.Amount;
			}
			else
			{	
				break;
			}
		}

		if(y - a > 0)
		{	
			return [..x[..fulls], new (x[fulls].Expiration, y - a)];
		}
		else
			return x[..fulls];
	}
}
