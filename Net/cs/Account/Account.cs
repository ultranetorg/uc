namespace Uccs.Net;

public class Account : IBinarySerializable
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public long							ECThis;
	public byte							ECThisPeriod;
	public long							ECNext;
	public long							BYBalance { get; set; }
	public int							LastTransactionNid { get; set; } = -1;
	//public int							LastEmissionId  { get; set; } = -1;
	public long							AverageUptime { get; set; }
	
	public long							BandwidthNext { get; set; }
	public Time							BandwidthExpiration { get; set; } = Time.Empty;
	public long							BandwidthToday { get; set; }
	public Time							BandwidthTodayTime { get; set; }
	public long							BandwidthTodayAvailable { get; set; }

	public override string ToString()
	{
		return $"{Id}, {Address}, ECThis={ECThis}, ECNext={ECNext}, BY={BYBalance}, LTNid={LastTransactionNid}, AverageUptime={AverageUptime}";
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Address);

		writer.Write7BitEncodedInt64(ECThis);
		writer.Write(ECThisPeriod);
		writer.Write7BitEncodedInt64(ECNext);
		writer.Write7BitEncodedInt64(BYBalance);

		writer.Write7BitEncodedInt(LastTransactionNid);
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

		ECThis	 			= reader.Read7BitEncodedInt64();
		ECThisPeriod 		= reader.ReadByte();
		ECNext	 			= reader.Read7BitEncodedInt64();
		BYBalance 			= reader.Read7BitEncodedInt64();

		LastTransactionNid	= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();
		BandwidthNext		= reader.Read7BitEncodedInt64();

		if(BandwidthNext > 0)
		{
			BandwidthExpiration		= reader.Read<Time>();
			BandwidthToday			= reader.Read7BitEncodedInt64();
			BandwidthTodayTime		= reader.Read<Time>();
			BandwidthTodayAvailable	= reader.Read7BitEncodedInt64();
		}
	}
}
