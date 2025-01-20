namespace Uccs.Smp;

public enum AuthorFlag : byte
{
	None, 
}

public class Author : IBinarySerializable
{
	public static readonly Time	RenewaPeriod = Time.FromDays(365);

	public EntityId				Id { get; set; }
	public string				Title { get; set; }
	public EntityId				Owner { get; set; }
	public Time					Expiration { get; set; }
	public short				SpaceReserved { get; set; }
	public short				SpaceUsed { get; set; }

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

	public static bool CanRenew(Author publisher, Account by, Time time)
	{
		return  publisher == null || 
				publisher != null && publisher.Owner == by.Id && time > publisher.Expiration - RenewaPeriod && /// renewal by owner: renewal is allowed during last year olny
																 time <= publisher.Expiration;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
	
		//writer.Write((byte)f);
		writer.Write(Title);
		writer.Write7BitEncodedInt(SpaceReserved);
		writer.Write7BitEncodedInt(SpaceUsed);
		writer.Write(Owner);
		writer.Write(Expiration);
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();

		//var f			= (AuthorFlag)reader.ReadByte();
		Title			= reader.ReadString();
		SpaceReserved	= (short)reader.Read7BitEncodedInt();
		SpaceUsed		= (short)reader.Read7BitEncodedInt();
		Owner			= reader.Read<EntityId>();
		Expiration		= reader.Read<Time>();
	}
}
