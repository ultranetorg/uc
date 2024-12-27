namespace Uccs.Fair;

public class ProductEntry : Product, ITableEntry
{
	public BaseId			BaseId => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public ProductEntry()
	{
	}

	public ProductEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public ProductEntry Clone()
	{
		return new(Mcv){Id = Id,
						AuthorId = AuthorId,
						Flags = Flags,
						Fields = Fields?.ToArray(),
						Updated = Updated};
	}

	public void WriteMain(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(AuthorId);
		writer.Write((byte)Flags);
		writer.Write(Updated);
		writer.Write(Fields);
	}

	public void ReadMain(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		AuthorId	= reader.Read<EntityId>();
		Flags		= (ProductFlags)reader.ReadByte();
		Updated		= reader.Read<Time>();
		Fields		= reader.ReadArray<ProductField>();
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

}

