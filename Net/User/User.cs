using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Uccs.Net;

public interface IHolder
{
	bool		IsPermitted(Execution executions, uint operation, AutoId signer);
}

public interface ISpacetimeHolder : IHolder
{
	long		Spacetime { get; set; }
	bool		Free { get; set; }

	public void WriteSpaceConsumer(Writer writer)
	{
		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write(Free);
	}

	public void ReadSpaceConsumer(Reader reader)
	{
		Spacetime	= reader.Read7BitEncodedInt64();
		Free	 	= reader.ReadBoolean();
	}

	public void Copy(ISpacetimeHolder a)
	{ 
		a.Spacetime	= Spacetime;
		a.Free		= Free;
	}
}

public interface ISpaceConsumer
{
	long		Space { get; set; }
	short		Expiration { get; set; }

	public void WriteSpaceConsumer(Writer writer)
	{
		writer.Write7BitEncodedInt64(Space);
		writer.Write(Expiration);
	}

	public void ReadSpaceConsumer(Reader reader)
	{
		Space	 	= reader.Read7BitEncodedInt64();
		Expiration 	= reader.ReadInt16();
	}

	public void Copy(ISpaceConsumer a)
	{ 
		a.Space			= Space;
		a.Expiration	= Expiration;
	}

	bool IsExpired(Time time) 
	{
		return time.Days > Expiration;
	}

	bool CanRenew(Time time, Time duration)
	{
		return Expiration + duration.Days - time.Days < Time.FromYears(Mcv.EntityRentYearsMax).Days;
	}
}

public interface IEnergyHolder : IHolder
{
	long		Energy { get; set; }
	byte		EnergyThisPeriod { get; set; }
	long		EnergyNext { get; set; }
	int			EnergyPeriod { get; set; } ///  1 hour
	int			EnergyRating { get; set; }
	
	int			Bandwidth { get; set; }
	int			BandwidthExpiration { get; set; }

	public void Copy(IEnergyHolder a)
	{ 
		a.Energy				= Energy;
		a.EnergyThisPeriod      = EnergyThisPeriod;
		a.EnergyNext            = EnergyNext;
		a.EnergyPeriod			= EnergyPeriod;
		a.EnergyRating			= EnergyRating;
			
		a.Bandwidth				= Bandwidth;
		a.BandwidthExpiration	= BandwidthExpiration;
	}

	public void WriteEnergyHolder(Writer writer)
	{
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt(EnergyPeriod);
		writer.Write7BitEncodedInt(EnergyRating);
	
		writer.Write7BitEncodedInt(Bandwidth);
		writer.Write7BitEncodedInt(BandwidthExpiration);
	}

	public void ReadEnergyHolder(Reader reader)
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

//public class Permission : IBinarySerializable
//{
//	public bool					Users { get; set; }
//	public AutoId[]				Users { get; set; }
//	public uint[]				Operations { get; set; }
//
//	public void Read(Reader reader)
//	{
//		Users 		= reader.ReadArray<AutoId>();
//		Operations 	= reader.ReadArray(() => reader.ReadUInt32());
//	}
//
//	public void Write(Writer writer)
//	{
//		writer.Write(Users);
//		writer.Write(Operations, i => writer.Write(i));
//	}
//}


public class User : ITableEntry<AutoId>, IBinarySerializable, IEnergyHolder, ISpacetimeHolder
{
	public AutoId			Id { get; set; }
	public string			Name { get; set; }
	public PublicKey		Key { get; set; }
	//public Permission[]		Permissions { get; set; }
	public int				LastNonce { get; set; } = -1;
	public int				LastOutward { get; set; } = -1;
	public long				AverageUptime { get; set; }
	
	public long				Spacetime { get; set; }
	public bool				Free { get; set; }
	
	public long				Energy { get; set; }
	public byte				EnergyThisPeriod { get; set; }
	public long				EnergyNext { get; set; }
	public int				EnergyPeriod { get; set; }
	public int				EnergyRating { get; set; }

	public int				Bandwidth { get; set; }
	public int				BandwidthExpiration { get; set; }

	public bool				Deleted { get; set; }

	Mcv						Mcv;

	public static bool		IsNameValid(string name) =>	name.Length is >= NameLengthMin and <= NemaLengthMax && NameRegex.Match(name).Success;
	public static byte[]	NameToBytes(string name) => Encoding.ASCII.GetBytes(name);
	public static string	BytesToName(byte[] bytes) => Encoding.ASCII.GetString(bytes); 

	public const int		NameLengthMin = 4;
	public const int		NemaLengthMax = 32;

	static readonly Regex	NameRegex = new ("^[a-z0-9_]+$", RegexOptions.Compiled);

	public override string ToString()
	{
		return $"{Name}, {Id}, {Key}, {nameof(Energy)}={Energy}, {nameof(EnergyNext)}={EnergyNext}, {nameof(Spacetime)}={Spacetime}, {nameof(LastNonce)}={LastNonce}, {nameof(AverageUptime)}={AverageUptime}";
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

	public bool IsPermitted(Execution executions, uint operation, AutoId signer)
	{
		return Id == signer;
		//return Permissions.Any(i => (i.Operations.Length == 0 || i.Operations.Contains(operation)) && i.Users.Contains(signer));
	}

	public virtual void Write(Writer writer)
	{
		writer.Write(Id);
		writer.WriteASCII(Name);
		writer.Write(Key);

		writer.Write7BitEncodedInt64(Spacetime);
		writer.Write7BitEncodedInt(LastNonce);
		writer.Write7BitEncodedInt(LastOutward);
		writer.Write7BitEncodedInt64(AverageUptime);

		(this as IEnergyHolder).WriteEnergyHolder(writer);
		(this as ISpacetimeHolder).WriteSpaceConsumer(writer);
	}

	public virtual void Read(Reader reader)
	{
		Id					= reader.Read<AutoId>();
		Name				= reader.ReadASCII();
		Key					= reader.Read<PublicKey>();

		Spacetime 			= reader.Read7BitEncodedInt64();
		LastNonce			= reader.Read7BitEncodedInt();
		LastOutward			= reader.Read7BitEncodedInt();
		AverageUptime		= reader.Read7BitEncodedInt64();

		(this as IEnergyHolder).ReadEnergyHolder(reader);
		(this as ISpacetimeHolder).ReadSpaceConsumer(reader);
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
		a.Name					= Name;
		a.Key					= Key;
		a.Spacetime				= Spacetime;
		a.LastNonce				= LastNonce;
		a.LastOutward			= LastOutward;
		a.AverageUptime			= AverageUptime;

		(this as IEnergyHolder).Copy(a);
		(this as ISpacetimeHolder).Copy(a);

		return a;
	}

	public virtual void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public virtual void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
