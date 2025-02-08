namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public class Author : IBinarySerializable
{
	public static readonly Time	RenewalPeriod = Time.FromYears(1);

	public EntityId				Id { get; set; }
	public string				Title { get; set; }
	public EntityId				Owner { get; set; }
	public Time					Expiration { get; set; }
	public int					SpaceReserved { get; set; }
	public int					SpaceUsed { get; set; }
	
	public long					BYBalance { get; set; }
	public long					ECThis { get; set; }
	public byte					ECThisYear { get; set; }
	public long					ECNext { get; set; }
	public int					ModerationReward  { get; set; }

	public static bool Valid(string name)
	{
		if(name == null)
			return false;

		return true;
	}

	public static bool IsOwner(Author domain, Account account, Time time)
	{
		return domain.Owner == account.Id && !IsExpired(domain, time);
	}

	public static bool IsExpired(Author a, Time time) 
	{
		return	a.Owner != null && time > a.Expiration;	 /// owner has not renewed, restart the auction
	}

	public static bool CanRenew(Author author, Account by, Time time)
	{
		return  author.Owner == by.Id && time > author.Expiration - RenewalPeriod && /// renewal by owner: renewal is allowed during last year olny
										 time <= author.Expiration;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Owner);
		writer.Write(Expiration);
		writer.Write7BitEncodedInt(SpaceReserved);
		writer.Write7BitEncodedInt(SpaceUsed);
		writer.Write7BitEncodedInt(ModerationReward);

		writer.Write7BitEncodedInt64(ECThis);
		writer.Write(ECThisYear);
		writer.Write7BitEncodedInt64(ECNext);
		writer.Write7BitEncodedInt64(BYBalance);
	}

	public void Read(BinaryReader reader)
	{
		Id					= reader.Read<EntityId>();
		Title				= reader.ReadString();
		Owner				= reader.Read<EntityId>();
		Expiration			= reader.Read<Time>();
		SpaceReserved		= reader.Read7BitEncodedInt();
		SpaceUsed			= reader.Read7BitEncodedInt();
		ModerationReward	= reader.Read7BitEncodedInt();

		ECThis	 			= reader.Read7BitEncodedInt64();
		ECThisYear 			= reader.ReadByte();
		ECNext	 			= reader.Read7BitEncodedInt64();
		BYBalance 			= reader.Read7BitEncodedInt64();
	}
}
