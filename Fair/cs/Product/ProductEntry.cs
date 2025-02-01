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
						Updated = Updated,
						Publications = Publications};
	}

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
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

