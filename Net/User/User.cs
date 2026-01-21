using System.Globalization;
using System.Text;

namespace Uccs.Net;

public interface IHolder
{
	bool		IsSpendingAuthorized(Execution executions, AutoId signer);
}

public interface ISpacetimeHolder : IHolder
{
	long		Spacetime { get; set; }
}

public interface ISpaceConsumer
{
	long		Space { get; set; }
	short		Expiration { get; set; }

	public void WriteSpaceConsumer(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(Space);
		writer.Write(Expiration);
	}

	public void ReadSpaceConsumer(BinaryReader reader)
	{
		Space	 	= reader.Read7BitEncodedInt64();
		Expiration 	= reader.ReadInt16();
	}
}

public interface IEnergyHolder : IHolder
{
	long		Energy { get; set; }
	byte		EnergyThisPeriod { get; set; }
	long		EnergyNext { get; set; }
	int			EnergyPeriod { get; set; }
	int			EnergyRating { get; set; }
	
	int			Bandwidth { get; set; }
	int			BandwidthExpiration { get; set; }

	public void Clone(IEnergyHolder a)
	{ 
		a.Energy				= Energy;
		a.EnergyThisPeriod      = EnergyThisPeriod;
		a.EnergyNext            = EnergyNext;
		a.EnergyPeriod			= EnergyPeriod;
		a.EnergyRating			= EnergyRating;
			
		a.Bandwidth				= Bandwidth;
		a.BandwidthExpiration	= BandwidthExpiration;
	}

	public void WriteEnergyHolder(BinaryWriter writer)
	{
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt(EnergyPeriod);
		writer.Write7BitEncodedInt(EnergyRating);
	
		writer.Write7BitEncodedInt(Bandwidth);
		writer.Write7BitEncodedInt(BandwidthExpiration);
	}

	public void ReadEnergyHolder(BinaryReader reader)
	{
		Energy	 			= reader.Read7BitEncodedInt64();
		EnergyThisPeriod 	= reader.ReadByte();
		EnergyNext	 		= reader.Read7BitEncodedInt64();
		EnergyPeriod		= reader.Read7BitEncodedInt();
		EnergyRating		= reader.Read7BitEncodedInt();

		Bandwidth				= reader.Read7BitEncodedInt();
		BandwidthExpiration		= reader.Read7BitEncodedInt();
	}
}

public class User : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ITableEntry
{
	public AutoId			Id { get; set; }
	public string			Name { get; set; }
	public AccountAddress	Owner { get; set; }
	public int				LastNonce { get; set; } = -1;
	public long				AverageUptime { get; set; }
	
	public long				Spacetime { get; set; }
	
	public long				Energy { get; set; }
	public byte				EnergyThisPeriod { get; set; }
	public long				EnergyNext { get; set; }
	public int				EnergyPeriod { get; set; }
	public int				EnergyRating { get; set; }

	public int				Bandwidth { get; set; }
	public int				BandwidthExpiration { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }

	Mcv						Mcv;

	public static byte[]	NameToBytes(string name) => Encoding.ASCII.GetBytes(name);
	public static string	BytesToName(byte[] bytes) => Encoding.ASCII.GetString(bytes); 

	public override string ToString()
	{
		return $"{Name}, {Id}, {Owner}, ECThis={Energy}, ECNext={EnergyNext}, BD={Spacetime}, LTNid={LastNonce}, AverageUptime={AverageUptime}";
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

	public bool IsSpendingAuthorized(Execution executions, AutoId signer)
	{
		return Id == signer;
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Owner);
		writer.WriteASCII(Name);

		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write7BitEncodedInt(LastNonce);
		writer.Write7BitEncodedInt64(AverageUptime);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public virtual void Read(BinaryReader reader)
	{
		Id					= reader.Read<AutoId>();
		Owner				= reader.ReadAccount();
		Name				= reader.ReadASCII();

		Spacetime 			= reader.Read7BitEncodedInt64();
		LastNonce			= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}

	public User()
	{
	}

	public User(Mcv mcv)
	{
		Mcv = mcv;
	}

	public virtual object Clone()
	{
		var a = Mcv.Users.Create();

		a.Id					= Id;
		a.Owner					= Owner;
		a.Name					= Name;
		a.Spacetime				= Spacetime;
		a.LastNonce				= LastNonce;
		a.AverageUptime			= AverageUptime;

		((IEnergyHolder)this).Clone(a);

		return a;
	}

	public virtual void WriteMain(BinaryWriter w)
	{
		Write(w);
	}

	public virtual void ReadMain(BinaryReader r)
	{
		Read(r);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
