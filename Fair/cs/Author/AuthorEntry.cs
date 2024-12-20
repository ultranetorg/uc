using System.Diagnostics;

namespace Uccs.Fair;

public class AuthorEntry : Author, ITableEntry
{
	//public bool				New;
	//public bool				Affected;
	Mcv						Mcv;
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }

	public AuthorEntry()
	{
	}

	public AuthorEntry(Mcv chain)
	{
		Mcv = chain;
	}

	public override string ToString()
	{
		return $"{Id}, {Owner}, {Expiration}";
	}

	public AuthorEntry Clone()
	{
		return new AuthorEntry(Mcv) {Id = Id,
										Owner = Owner,
										Expiration = Expiration,
										SpaceReserved = SpaceReserved,
										SpaceUsed = SpaceUsed,
										NextProductId = NextProductId};
	}

	public void WriteMain(BinaryWriter writer)
	{
		var f = PublisherFlag.None;

		writer.Write((byte)f);
		writer.Write7BitEncodedInt(SpaceReserved);
		writer.Write7BitEncodedInt(SpaceUsed);
		writer.Write(Owner);
		writer.Write(Expiration);
		writer.Write7BitEncodedInt(NextProductId);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void ReadMain(BinaryReader reader)
	{
		var f			= (PublisherFlag)reader.ReadByte();
		SpaceReserved	= (short)reader.Read7BitEncodedInt();
		SpaceUsed		= (short)reader.Read7BitEncodedInt();
		Owner			= reader.Read<EntityId>();
		Expiration		= reader.Read<Time>();
		NextProductId	= reader.Read7BitEncodedInt();

	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

}
