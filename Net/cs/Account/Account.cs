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
}

public class Account : IBinarySerializable
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public List<EC>						ECBalance { get; set; }
	public long							BYBalance { get; set; }
	public int							LastTransactionNid { get; set; } = -1;
	//public int							LastEmissionId  { get; set; } = -1;
	public long							AverageUptime { get; set; }
	
	public long							BandwidthNext { get; set; }
	public Time							BandwidthExpiration { get; set; } = Time.Empty;
	public long							BandwidthToday { get; set; }
	public Time							BandwidthTodayTime { get; set; }
	public long							BandwidthTodayAvailable { get; set; }

	public long							Integrate(Time time) => ECBalance?.SkipWhile(i => i.Expiration < time).Sum(i => i.Amount) ?? 0;
	public static long					Integrate(List<EC> ecs, Time time) => ecs.SkipWhile(i => i.Expiration < time).Sum(i => i.Amount);

	public override string ToString()
	{
		return $"{Id}, {Address}, EC={{{string.Join(',', ECBalance?.Select(i => i.Amount) ?? [])}}}, BY={BYBalance}, LTNid={LastTransactionNid}, AverageUptime={AverageUptime}";
	}

	public void ECBalanceAdd(IEnumerable<EC> ec)
	{
		foreach(var i in ec)
		{
			ECBalanceAdd(i);
		}
	}

	public void ECBalanceAdd(EC reserve)
	{
		ECBalance = ECBalance ?? [];

		var e = ECBalance.FindIndex(j => j.Expiration == reserve.Expiration);

		if(e == -1)
		{
			var p = ECBalance.FindIndex(j => j.Expiration > reserve.Expiration);

			if(p == -1)
				ECBalance.Add(reserve);
			else
				ECBalance.Insert(p, reserve);
		} 
		else
		{
			ECBalance[e] = new (ECBalance[e].Expiration, ECBalance[e].Amount + reserve.Amount);
		}
	}

	public void ECBalanceSubtract(Time expiration, long x)
	{
		int c = 0;

		foreach(var i in ECBalance?.Where(i => i.Expiration >= expiration) ?? [])
		{
			x -= i.Amount;

			if(x >= 0)
			{
				c++;
			}
			else
			{
				break;
			}
		}

		if(c > 0)
			ECBalance.RemoveRange(0, c);

		if(x < 0)
			ECBalance[0] = new (ECBalance[0].Expiration, -x);
	}

	public EC[] ECBalanceDifference(Time expiration, long x)
	{
		int fulls = 0;
		long a = 0;

		foreach(var i in ECBalance?.Where(i => i.Expiration >= expiration) ?? [])
		{
			if(a + i.Amount <= x)
			{
				fulls++;
				a += i.Amount;
			}
			else
			{	
				break;
			}
		}

		var d = ECBalance.GetRange(0, fulls).ToArray();

		if(x - a > 0)
		{	
			d = [..d, new (ECBalance[fulls].Expiration, x - a)];
		}

		return d;
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Address);
		writer.Write(ECBalance);
		writer.Write7BitEncodedInt64(BYBalance);
		writer.Write7BitEncodedInt(LastTransactionNid);
		//writer.Write7BitEncodedInt(LastEmissionId);
		writer.Write7BitEncodedInt64(AverageUptime);
		
		writer.Write7BitEncodedInt64(BandwidthNext);
		
		if(BandwidthNext > 0)
		{
			writer.Write(BandwidthExpiration);
			writer.Write7BitEncodedInt64(BandwidthToday);
			writer.Write(BandwidthTodayTime);
			writer.Write7BitEncodedInt64(BandwidthTodayAvailable);
		}
	}

	public virtual void Read(BinaryReader reader)
	{
		Id					= reader.Read<EntityId>();
		Address				= reader.ReadAccount();
		ECBalance 			= reader.ReadList<EC>();
		BYBalance 			= reader.Read7BitEncodedInt64();
		LastTransactionNid	= reader.Read7BitEncodedInt();
		//LastEmissionId		= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();

		BandwidthNext = reader.Read7BitEncodedInt64();

		if(BandwidthNext > 0)
		{
			BandwidthExpiration		= reader.Read<Time>();
			BandwidthToday			= reader.Read7BitEncodedInt64();
			BandwidthTodayTime		= reader.Read<Time>();
			BandwidthTodayAvailable	= reader.Read7BitEncodedInt64();
		}
	}
}
