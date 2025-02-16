using NBitcoin.Secp256k1;

namespace Uccs.Fair;

public enum AuthorFlag : byte
{
	None, 
}

public class Author : IBinarySerializable, IEnergyHolder, ISpaceHolder, ISpaceConsumer
{
	public static readonly Time	RenewalPeriod = Time.FromYears(1);

	public EntityId				Id { get; set; }
	public string				Title { get; set; }
	public EntityId				Owner { get; set; }

	public Time					Expiration { get; set; }
	public long					Space { get; set; }
	public long					Spacetime { get; set; }
	public long					Energy { get; set; }
	public long					EnergyNext { get; set; }
	public byte					EnergyThisPeriod { get; set; }
	public long					ModerationReward  { get; set; }

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
		writer.Write7BitEncodedInt64(ModerationReward);

		writer.Write(Expiration);
		writer.Write7BitEncodedInt64(Space);
		writer.Write7BitEncodedInt64(Energy);
		writer.Write(EnergyThisPeriod);
		writer.Write7BitEncodedInt64(EnergyNext);
		writer.Write7BitEncodedInt64(Spacetime);
	}

	public void Read(BinaryReader reader)
	{
		Id					= reader.Read<EntityId>();
		Title				= reader.ReadString();
		Owner				= reader.Read<EntityId>();
		ModerationReward	= reader.Read7BitEncodedInt64();

		Expiration			= reader.Read<Time>();
		Space				= reader.Read7BitEncodedInt64();
		Energy		 		= reader.Read7BitEncodedInt64();
		EnergyThisPeriod 	= reader.ReadByte();
		EnergyNext	 		= reader.Read7BitEncodedInt64();
		Spacetime		 	= reader.Read7BitEncodedInt64();
	}
}
