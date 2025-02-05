namespace Uccs.Net;

public class Account : IBinarySerializable
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public EC[]							ECBalance { get; set; }
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
		ECBalance 			= reader.ReadArray<EC>();
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
