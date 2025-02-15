﻿using System.Globalization;

namespace Uccs.Net;

public interface ISpaceHolder
{
	long		Spacetime { get; set; }
}

public interface ISpaceConsumer
{
	long		Space { get; set; }
	Time		Expiration { get; set; }
}

public interface IEnergyHolder
{
	long		Energy { get; set; }
	byte		EnergyThisPeriod { get; set; }
	long		EnergyNext { get; set; }
}

public class Account : IBinarySerializable, IEnergyHolder, ISpaceHolder
{
	public EntityId						Id { get; set; }
	public AccountAddress				Address { get; set; }
	public long							Spacetime { get; set; }
	public long							Energy { get; set; }
	public byte							EnergyThisPeriod { get; set; }
	public long							EnergyNext { get; set; }
	public int							LastTransactionNid { get; set; } = -1;
	public long							AverageUptime { get; set; }
	
	public long							Bandwidth { get; set; }
	public Time							BandwidthExpiration { get; set; } = Time.Empty;
	public long							BandwidthToday { get; set; }
	public Time							BandwidthTodayTime { get; set; }
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

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Address);

		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);

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

		Spacetime 			= reader.Read7BitEncodedInt64();
		Energy	 			= reader.Read7BitEncodedInt64();
		EnergyThisPeriod 	= reader.ReadByte();
		EnergyNext	 		= reader.Read7BitEncodedInt64();

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
