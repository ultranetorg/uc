using System.Globalization;

namespace Uccs.Net;

public class Account : IBinarySerializable
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public long							EC;
	public byte							ECThisPeriod;
	public long							ECNext;
	public long							BDBalance { get; set; }
	public int							LastTransactionNid { get; set; } = -1;
	//public int							LastEmissionId  { get; set; } = -1;
	public long							AverageUptime { get; set; }
	
	public long							Bandwidth { get; set; }
	public Time							BandwidthExpiration { get; set; } = Time.Empty;
	public long							BandwidthToday { get; set; }
	public Time							BandwidthTodayTime { get; set; }
	public long							BandwidthTodayAvailable { get; set; }

	public override string ToString()
	{
		return $"{Id}, {Address}, ECThis={EC}, ECNext={ECNext}, BD={BDBalance}, LTNid={LastTransactionNid}, AverageUptime={AverageUptime}";
	}

	public static long ParseSpacetime(string t)
	{
		t = t.Replace(" ", null).Replace("\t", null).ToUpper();

		if(t.EndsWith("BD"))	return long.Parse(t.Substring(0, t.Length-2), NumberStyles.AllowThousands);
		if(t.EndsWith("BW"))	return long.Parse(t.Substring(0, t.Length-2), NumberStyles.AllowThousands) * 7;
		if(t.EndsWith("BM"))	return long.Parse(t.Substring(0, t.Length-2), NumberStyles.AllowThousands) * 30;
		if(t.EndsWith("BY"))	return long.Parse(t.Substring(0, t.Length-2), NumberStyles.AllowThousands) * 365;

		throw new FormatException(t);
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Address);

		writer.Write7BitEncodedInt64(EC);
		writer.Write(ECThisPeriod);
		writer.Write7BitEncodedInt64(ECNext);
		writer.Write7BitEncodedInt64(BDBalance);

		writer.Write7BitEncodedInt(LastTransactionNid);
		writer.Write7BitEncodedInt64(AverageUptime);

		writer.Write7BitEncodedInt64(Bandwidth);
		
		if(Bandwidth > 0)
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

		EC	 				= reader.Read7BitEncodedInt64();
		ECThisPeriod 		= reader.ReadByte();
		ECNext	 			= reader.Read7BitEncodedInt64();
		BDBalance 			= reader.Read7BitEncodedInt64();

		LastTransactionNid	= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();
		Bandwidth			= reader.Read7BitEncodedInt64();

		if(Bandwidth > 0)
		{
			BandwidthExpiration		= reader.Read<Time>();
			BandwidthToday			= reader.Read7BitEncodedInt64();
			BandwidthTodayTime		= reader.Read<Time>();
			BandwidthTodayAvailable	= reader.Read7BitEncodedInt64();
		}
	}
}
