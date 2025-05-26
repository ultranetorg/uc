namespace Uccs.Fair;

public enum PublicationFlags : byte
{
	None,
	CreatedByAuthor	= 0b0000_0000,
	CreatedBySite	= 0b0000_0001,
}

public class Publication : IBinarySerializable, ITableEntry
{
	public AutoId							Id { get; set; }
	public AutoId							Site { get; set; }
	public AutoId							Category { get; set; }
	public AutoId							Creator { get; set; }
	public AutoId							Product { get; set; }
	//public PublicationStatus				Status { get; set; }
	public ProductFieldVersionReference[]	Fields { get; set; }
	public AutoId[]							Reviews { get; set; }
	public AutoId[]							ReviewChanges { get; set; }
	public PublicationFlags					Flags { get; set; }

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
		return $"{Id}, Product={Product}, Category={Category}, Creator={Creator}, Fields={Fields.Length}, Flags={Flags}";
	}

	public object Clone()
	{
		return new Publication(Mcv){Id				= Id,
									Site			= Site,
									Category		= Category,
									Creator			= Creator,
									Product			= Product,
									Fields			= Fields,
									Reviews			= Reviews,
									ReviewChanges	= ReviewChanges,
									Flags			= Flags};
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
		Creator			= reader.Read<AutoId>();
		Product			= reader.Read<AutoId>();
		Fields			= reader.ReadArray<ProductFieldVersionReference>();
		Reviews			= reader.ReadArray<AutoId>();
		ReviewChanges	= reader.ReadArray<AutoId>();
		Flags			= reader.Read<PublicationFlags>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Site);
		writer.WriteNullable(Category);
		writer.Write(Creator);
		writer.Write(Product);
		writer.Write(Fields);
		writer.Write(Reviews);
		writer.Write(ReviewChanges);
		writer.Write(Flags);
	}
}
