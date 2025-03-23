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
	public EntityId							Id { get; set; }
	public EntityId							Category { get; set; }
	public EntityId							Creator { get; set; }
	public EntityId							Product { get; set; }
	public PublicationStatus				Status { get; set; }
	public ProductFieldVersionReference[]	Fields { get; set; }
	public ProductFieldVersionReference[]	Changes { get; set; }
	public EntityId[]						Reviews { get; set; }
	public EntityId[]						ReviewChanges { get; set; }
	public PublicationFlags					Flags { get; set; }

	public BaseId							Key => Id;
	public bool								Deleted { get; set; }
	FairMcv									Mcv;

	public Publication()
	{
	}

	public Publication(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Publication Clone()
	{
		return new(Mcv){Id				= Id,
						Category		= Category,
						Creator			= Creator,
						Product			= Product,
						Status			= Status,
						Fields			= Fields,
						Changes			= Changes,
						Reviews			= Reviews,
						ReviewChanges	= ReviewChanges,
						Flags			= Flags,
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
		Id				= reader.Read<EntityId>();
		Category		= reader.Read<EntityId>();
		Creator			= reader.Read<EntityId>();
		Product			= reader.Read<EntityId>();
		Status			= reader.Read<PublicationStatus>();
		Fields			= reader.ReadArray<ProductFieldVersionReference>();
		Changes			= reader.ReadArray<ProductFieldVersionReference>();
		Reviews			= reader.ReadArray<EntityId>();
		ReviewChanges	= reader.ReadArray<EntityId>();
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
