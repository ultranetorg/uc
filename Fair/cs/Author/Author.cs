using NBitcoin.Secp256k1;

namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public class Author : IBinarySerializable, IEnergyHolder, ISpacetimeHolder, ISpaceConsumer
{
	public static readonly short	RenewalPeriod = (short)Time.FromYears(1).Days;

	public EntityId				Id { get; set; }
	public string				Title { get; set; }
	public EntityId[]			Owners { get; set; }

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

	public static bool Valid(string name)
	{
		if(name == null)
			return false;

		return true;
	}

	public bool IsSpendingAuthorized(Round round, EntityId signer)
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

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Owners);
		writer.Write7BitEncodedInt64(ModerationReward);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Spacetime);

		((IEnergyHolder)this).WriteEnergyHolder(writer);
	}

	public void Read(BinaryReader reader)
	{
		Id					= reader.Read<EntityId>();
		Title				= reader.ReadString();
		Owners				= reader.ReadArray<EntityId>();
		ModerationReward	= reader.Read7BitEncodedInt64();

		Expiration			= reader.ReadInt16();
		Space				= reader.Read7BitEncodedInt64();
		Spacetime		 	= reader.Read7BitEncodedInt64();

		((IEnergyHolder)this).ReadEnergyHolder(reader);
	}
}
