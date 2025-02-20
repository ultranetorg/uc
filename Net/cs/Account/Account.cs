using System.Globalization;

namespace Uccs.Net;

public interface IHolder
{
	bool		IsSpendingAuthorized(Round round, EntityId signer);
}

public interface ISpacetimeHolder : IHolder
{
	long		Spacetime { get; set; }
}

public interface ISpaceConsumer
{
	long		Space { get; set; }
	short		Expiration { get; set; }
}

public interface IEnergyHolder : IHolder
{
	long		Energy { get; set; }
	byte		EnergyThisPeriod { get; set; }
	long		EnergyNext { get; set; }
	
	long		Bandwidth { get; set; }
	short		BandwidthExpiration { get; set; }
	long		BandwidthToday { get; set; }
	short		BandwidthTodayTime { get; set; }
	long		BandwidthTodayAvailable { get; set; }

	public void Clone(IEnergyHolder a)
	{ 
		a.Energy				  = Energy;
		a.EnergyThisPeriod        = EnergyThisPeriod;
		a.EnergyNext              = EnergyNext;
		a.Bandwidth               = Bandwidth;
		a.BandwidthExpiration     = BandwidthExpiration;
		a.BandwidthToday          = BandwidthToday;
		a.BandwidthTodayTime      = BandwidthTodayTime;
		a.BandwidthTodayAvailable = BandwidthTodayAvailable;
	}

	public void WriteEnergyHolder(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt64(Bandwidth);
		
		if(Bandwidth > 0)
		{
			writer.Write(BandwidthExpiration);
			writer.Write7BitEncodedInt64(BandwidthToday);
			writer.Write(BandwidthTodayTime);
			writer.Write7BitEncodedInt64(BandwidthTodayAvailable);
		}
	}

	public void ReadEnergyHolder(BinaryReader reader)
	{
		Energy	 			= reader.Read7BitEncodedInt64();
		EnergyThisPeriod 	= reader.ReadByte();
		EnergyNext	 		= reader.Read7BitEncodedInt64();
		Bandwidth			= reader.Read7BitEncodedInt64();

		if(Bandwidth > 0)
		{
			BandwidthExpiration		= reader.ReadInt16();
			BandwidthToday			= reader.Read7BitEncodedInt64();
			BandwidthTodayTime		= reader.ReadInt16();
			BandwidthTodayAvailable	= reader.Read7BitEncodedInt64();
		}
	}
}

public class Account : IBinarySerializable, IEnergyHolder, ISpacetimeHolder
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public long							Spacetime { get; set; }
	public int							LastTransactionNid { get; set; } = -1;
	public long							AverageUptime { get; set; }
	
	public long							Energy { get; set; }
	public byte							EnergyThisPeriod { get; set; }
	public long							EnergyNext { get; set; }
	public long							Bandwidth { get; set; }
	public short						BandwidthExpiration { get; set; } = -1;
	public long							BandwidthToday { get; set; }
	public short						BandwidthTodayTime { get; set; }
	public long							BandwidthTodayAvailable { get; set; }

	public override string ToString()
	{
		return $"{Id}, {Address}, ECThis={Energy}, ECNext={EnergyNext}, BD={Spacetime}, LTNid={LastTransactionNid}, AverageUptime={AverageUptime}";
	}
	public static long ParseSpacetime(string t)
	{
		t = t.Replace(" ", null).Replace("\t", null).ToUpper();

		if(t.EndsWith("BD")) return long.Parse(t.Substring(0, t.Length - 2), NumberStyles.AllowThousands);
		if(t.EndsWith("BW")) return long.Parse(t.Substring(0, t.Length - 2), NumberStyles.AllowThousands) * 7;
		if(t.EndsWith("BM")) return long.Parse(t.Substring(0, t.Length - 2), NumberStyles.AllowThousands) * 30;
		if(t.EndsWith("BY")) return long.Parse(t.Substring(0, t.Length - 2), NumberStyles.AllowThousands) * 365;

		return long.Parse(t, NumberStyles.AllowThousands);
	}

	public bool IsSpendingAuthorized(Round round, EntityId signer)
	{
		return Id == signer;
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Address);

		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write7BitEncodedInt(LastTransactionNid);
		writer.Write7BitEncodedInt64(AverageUptime);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public virtual void Read(BinaryReader reader)
	{
		Id					= reader.Read<EntityId>();
		Address				= reader.ReadAccount();

		Spacetime 			= reader.Read7BitEncodedInt64();
		LastTransactionNid	= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}


}
