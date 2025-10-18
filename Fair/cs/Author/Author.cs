using NBitcoin.Secp256k1;

namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public enum AuthorLink : byte
{
	Custom, Website, Youtube, Facebook, X, Github, Linkedin, Instagram, 
}

public class Author : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry, IExpirable
{
	//public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;

	public AutoId					Id { get; set; }
	public string					Nickname { get; set; }
	public string					Title { get; set; }
	public string					Description { get; set; }
	public AutoId[]					Owners { get; set; }
	public AutoId					Avatar  { get; set; }

	public short					Expiration { get; set; }
	public long						Space { get; set; }
	public long						Spacetime { get; set; }
	public long						ModerationReward  { get; set; }

	public long						Energy { get; set; }
	public byte						EnergyThisPeriod { get; set; }
	public long						EnergyNext { get; set; }
	public long						Bandwidth { get; set; }
	public short					BandwidthExpiration { get; set; } = -1;
	public long						BandwidthToday { get; set; }
	public short					BandwidthTodayTime { get; set; }
	public long						BandwidthTodayAvailable { get; set; }
	
	public AutoId[]					Products { get; set; }
	public AutoId[]					Sites { get; set; }
	public string[]					Links { get; set; }
	public AutoId[]					Files  { get; set; }

	public EntityId					Key => Id;
	Mcv								Mcv;
	public bool						Deleted { get; set; }

	public Author()
	{
	}

	public Author(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Id}, {Title}{(Nickname != "" ? $"({Nickname})" : null)}, Owners={Owners.Length}, Expiration={Expiration}";
	}

	public object Clone()
	{
		var a = new Author(Mcv)
				{				
					Id					= Id,
					Nickname			= Nickname,
					Title				= Title,
					Description			= Description,
					Owners				= Owners,
					Avatar				= Avatar,

					Expiration			= Expiration,
					Space				= Space,
					Spacetime			= Spacetime,
					ModerationReward	= ModerationReward,

					Products			= Products,
					Sites				= Sites,
					Links				= Links,
					Files				= Files,
				};

		((IEnergyHolder)this).Clone(a);

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
		return Owners.Contains(signer); /// TODO : Owner only
	}

	public static bool IsOwner(Author author, Account account, Time time)
	{
		return author.Owners.Contains(account.Id) && !author.IsExpired(time);
	}

	public bool IsExpired(Time time) 
	{
		return	time.Days > Expiration;	 /// owner has not renewed, restart the auction
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
		
		writer.Write(Products);
		writer.Write(Sites);
		writer.Write(Links);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
		
		Products	= reader.ReadArray<AutoId>();
		Sites		= reader.ReadArray<AutoId>();
		Links		= reader.ReadStrings();
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Nickname);
		writer.WriteUtf8(Title);
		writer.WriteUtf8Nullable(Description);
		writer.Write(Owners);
		writer.Write7BitEncodedInt64(ModerationReward);
		writer.WriteNullable(Avatar);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		writer.Write(Files);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public void Read(BinaryReader reader)
	{
		Id					= reader.Read<AutoId>();
		Nickname			= reader.ReadUtf8();
		Title				= reader.ReadUtf8();
		Description			= reader.ReadUtf8Nullable();
		Owners				= reader.ReadArray<AutoId>();
		ModerationReward	= reader.Read7BitEncodedInt64();
		Avatar				= reader.ReadNullable<AutoId>();

		Expiration			= reader.ReadInt16();
		Space				= reader.Read7BitEncodedInt64();
		Spacetime		 	= reader.Read7BitEncodedInt64();

		Files				= reader.ReadArray<AutoId>();
		
		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}
}
