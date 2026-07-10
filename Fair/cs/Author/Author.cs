using System.Text;

namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public enum AuthorLink : byte
{
	Custom, Website, Youtube, Facebook, X, Github, Linkedin, Instagram, 
}

public class UriReference : IBinarySerializable
{
	public string	Text { get; set; }
	public string	Uri { get; set; }

	public UriReference()
	{
	}

	public UriReference(string text, string value)
	{
		Text = text;
		Uri = value;
	}

	public void Read(Reader reader)
	{
		Text = reader.ReadUtf8();
		Uri = reader.ReadUtf8();
	}

	public void Write(Writer writer)
	{
		writer.WriteUtf8(Text);
		writer.WriteUtf8(Uri);
	}
}


public class Author : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry, IExpirable
{
	//public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;
	public const int				WeblinkLength = 1024;
	public const int				WeblinkCountMaximum = 10;

	public AutoId					Id { get; set; }
	public string					Name { get; set; }
	public string					Title { get; set; }
	public string					Description { get; set; }
	public string					VerifiedWebdomain { get; set; }
	public int						VerifiedWebdomainRank { get; set; }
	public AutoId[]					Owners { get; set; }
	public AutoId					Avatar  { get; set; }

	public short					Expiration { get; set; }
	public long						Space { get; set; }
	public long						Spacetime { get; set; }
	public long						ModerationReward  { get; set; }
	public bool						Free { get; set; }
	
	public AutoId[]					Products { get; set; }
	public AutoId[]					Stores { get; set; }
	public UriReference[]			References { get; set; }
	public AutoId[]					Files  { get; set; }
	
	public long						Energy { get; set; }
	public byte						EnergyThisPeriod { get; set; }
	public long						EnergyNext { get; set; }
	public int						EnergyPeriod { get; set; }
	public int						EnergyRating { get; set; }

	public int						Bandwidth { get; set; }
	public int						BandwidthExpiration { get; set; }

	public EntityId					Key => Id;
	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public static string			HashifyWebdomain(string webdomain) => Encoding.ASCII.GetBytes(webdomain).Aggregate((a, i) => (byte)(a ^ i)).ToString("X2");

	public Author()
	{
	}

	public Author(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {Title}{(Name != "" ? $"({Name})" : null)}, Owners={Owners.Length}, Expiration={Expiration}";
	}

	public object Clone()
	{
		var a = new Author(Mcv)
				{				
					Id						= Id,
					Name					= Name,
					Title					= Title,
					Description				= Description,
					VerifiedWebdomain		= VerifiedWebdomain,
					VerifiedWebdomainRank	= VerifiedWebdomainRank,
					Owners					= Owners,
					Avatar					= Avatar,

					Expiration				= Expiration,
					Space					= Space,
					Spacetime				= Spacetime,
					ModerationReward		= ModerationReward,

					Products				= Products,
					Stores					= Stores,
					References				= References,
					Files					= Files,
				};

		((IEnergyHolder)this).Copy(a);

		return a;
	}

	public static bool Valid(string name)
	{
		if(name == null)
			return false;

		return true;
	}

	public bool IsSpendingAuthorized(Execution executions, AutoId signer)
	{
		return Owners.Contains(signer);
	}

	public static bool IsOwner(Author author, User account, Time time)
	{
		return author.Owners.Contains(account.Id) && !author.IsExpired(time);
	}

	public bool IsExpired(Time time) 
	{
		return	time.Days > Expiration;	 /// owner has not renewed, restart the auction
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
		
		writer.Write(Products);
		writer.Write(Stores);
		writer.Write(References);
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
		
		Products	= reader.ReadArray<AutoId>();
		Stores		= reader.ReadArray<AutoId>();
		References	= reader.ReadArray<UriReference>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Name);
		writer.WriteUtf8(Title);
		writer.WriteUtf8(Description);
		writer.WriteUtf8(VerifiedWebdomain);
		writer.Write7BitEncodedInt(VerifiedWebdomainRank);
		writer.Write(Owners);
		writer.Write7BitEncodedInt64(ModerationReward);
		writer.WriteNullable(Avatar);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write(Files);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public void Read(Reader reader)
	{
		Id						= reader.Read<AutoId>();
		Name					= reader.ReadUtf8();
		Title					= reader.ReadUtf8();
		Description				= reader.ReadUtf8();
		VerifiedWebdomain		= reader.ReadUtf8();
		VerifiedWebdomainRank	= reader.Read7BitEncodedInt();
		Owners					= reader.ReadArray<AutoId>();
		ModerationReward		= reader.Read7BitEncodedInt64();
		Avatar					= reader.ReadNullable<AutoId>();

		Expiration				= reader.ReadInt16();
		Space					= reader.Read7BitEncodedInt64();
		Spacetime		 		= reader.Read7BitEncodedInt64();

		Files					= reader.ReadArray<AutoId>();
		
		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}
}
