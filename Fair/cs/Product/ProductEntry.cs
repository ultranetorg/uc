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
						Flags = Flags,
						Fields = Fields?.ToArray(),
						Updated = Updated};
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

