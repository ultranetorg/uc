namespace Uccs.Fair;

public enum PublicationFlags : byte
{
	None,
	ApprovedByAuthor	= 0b0000_0001,
	//ApprovedByModerator	= 0b0000_0010,
}

public class Publication : IBinarySerializable, ITableEntry
{
	public AutoId							Id { get; set; }
	public AutoId							Site { get; set; }
	public AutoId							Category { get; set; }
	//public AutoId							Creator { get; set; }
	public AutoId							Product { get; set; }
	public int								ProductVersion { get; set; }
	public AutoId[]							Reviews { get; set; }
	public PublicationFlags					Flags { get; set; }
	public byte								Rating { get; set; }

	public EntityId							Key => Id;
	public bool								Deleted { get; set; }
	FairMcv									Mcv;

	public Publication()
	{
	}

	public Publication(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, Product={Product}, Category={Category}, ProductVersion={ProductVersion}, Flags={Flags}";
	}

	public object Clone()
	{
		return	new Publication(Mcv)
				{
					Id				= Id,
					Site			= Site,
					Category		= Category,
					Product			= Product,
					ProductVersion	= ProductVersion,
					Reviews			= Reviews,
					Flags			= Flags,
					Rating			= Rating
				};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<AutoId>();
		Site			= reader.Read<AutoId>();
		Category		= reader.ReadNullable<AutoId>();
		Product			= reader.Read<AutoId>();
		ProductVersion	= reader.Read7BitEncodedInt();
		Reviews			= reader.ReadArray<AutoId>();
		Flags			= reader.Read<PublicationFlags>();
		Rating			= reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteNullable(Category);
		writer.Write(Product);
		writer.Write7BitEncodedInt(ProductVersion);
		writer.Write(Reviews);
		writer.Write(Flags);
		writer.Write(Rating);
	}
}
