using NBitcoin.Secp256k1;

namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public class Author : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer, ITableEntry
{
	public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;

	public AutoId				Id { get; set; }
	public string				Nickname { get; set; }
	public string				Title { get; set; }
	public AutoId[]				Owners { get; set; }
	public AutoId				Avatar  { get; set; }

	public short				Expiration { get; set; }
	public long					Space { get; set; }
	public long					Spacetime { get; set; }
	public long					ModerationReward  { get; set; }

	public long					Energy { get; set; }
	public byte					EnergyThisPeriod { get; set; }
	public long					EnergyNext { get; set; }
	public long					Bandwidth { get; set; }
	public short				BandwidthExpiration { get; set; } = -1;
	public long					BandwidthToday { get; set; }
	public short				BandwidthTodayTime { get; set; }
	public long					BandwidthTodayAvailable { get; set; }
	
	public AutoId[]				Products { get; set; }
	public AutoId[]				Sites { get; set; }
	//public AutoId[]			Files  { get; set; }

	public EntityId				Key => Id;
	Mcv							Mcv;
	public bool					Deleted { get; set; }

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
		var a = new Author(Mcv){Id					= Id,
								Nickname			= Nickname,
								Title				= Title,
								Owners				= Owners,
								Avatar				= Avatar,

								Expiration			= Expiration,
								Space				= Space,
								Spacetime			= Spacetime,
								ModerationReward	= ModerationReward,

								Products			= Products,
								Sites				= Sites,
								//Files				= Files
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

	public bool IsSpendingAuthorized(Execution round, AutoId signer)
	{
		return Owners.Contains(signer); /// TODO : Owner only
	}

	public static bool IsOwner(Author author, Account account, Time time)
	{
		return author.Owners.Contains(account.Id) && !IsExpired(author, time);
	}

	public static bool IsExpired(Author a, Time time) 
	{
		return	time.Days > a.Expiration;	 /// owner has not renewed, restart the auction
	}

	public static bool CanRenew(Author author, Account by, Time time)	
	{
		return IsOwner(author, by, time) && time.Days > author.Expiration - RenewalPeriod && /// renewal by owner: renewal is allowed during last year olny
											time.Days <= author.Expiration;
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
		
		writer.Write(Products);
		writer.Write(Sites);
		//writer.Write(Files);
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
		
		Products= reader.ReadArray<AutoId>();
		Sites	= reader.ReadArray<AutoId>();
		//Files	= reader.ReadArray<AutoId>();
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteUtf8(Nickname);
		writer.WriteUtf8(Title);
		writer.Write(Owners);
		writer.Write7BitEncodedInt64(ModerationReward);
		writer.WriteNullable(Avatar);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public void Read(BinaryReader reader)
	{
		Id					= reader.Read<AutoId>();
		Nickname			= reader.ReadUtf8();
		Title				= reader.ReadUtf8();
		Owners				= reader.ReadArray<AutoId>();
		ModerationReward	= reader.Read7BitEncodedInt64();
		Avatar				= reader.ReadNullable<AutoId>();

		Expiration			= reader.ReadInt16();
		Space				= reader.Read7BitEncodedInt64();
		Spacetime		 	= reader.Read7BitEncodedInt64();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}
}
