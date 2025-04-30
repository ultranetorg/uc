namespace Uccs.Fair;

public enum PublicationStatus : byte
{
	None,
	Pending,
	Approved,
	Rejected
	//Disputed
}

public enum PublicationFlags : byte
{
	None,
	CreatedByAuthor	= 0b0000_0000,
	CreatedBySite	= 0b0000_0001,
}

public class Publication : IBinarySerializable, ITableEntry
{
	public AutoId							Id { get; set; }
	public AutoId							Category { get; set; }
	public AutoId							Creator { get; set; }
	public AutoId							Product { get; set; }
	public PublicationStatus				Status { get; set; }
	public ProductFieldVersionReference[]	Fields { get; set; }
	public ProductFieldVersionReference[]	Changes { get; set; }
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
		return $"{Id}, Product={Product}, Category={Category}, Creator={Creator}, Fields={Fields.Length}, Changes={Changes.Length}, Flags={Flags}";
	}

	public object Clone()
	{
		return new Publication(Mcv){Id				= Id,
									Category		= Category,
									Creator			= Creator,
									Product			= Product,
									Status			= Status,
									Fields			= Fields,
									Changes			= Changes,
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
		Category		= reader.Read<AutoId>();
		Creator			= reader.Read<AutoId>();
		Product			= reader.Read<AutoId>();
		Status			= reader.Read<PublicationStatus>();
		Fields			= reader.ReadArray<ProductFieldVersionReference>();
		Changes			= reader.ReadArray<ProductFieldVersionReference>();
		Reviews			= reader.ReadArray<AutoId>();
		ReviewChanges	= reader.ReadArray<AutoId>();
		Flags			= reader.Read<PublicationFlags>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Category);
		writer.Write(Creator);
		writer.Write(Product);
		writer.Write(Status);
		writer.Write(Fields);
		writer.Write(Changes);
		writer.Write(Reviews);
		writer.Write(ReviewChanges);
		writer.Write(Flags);
	}
}
