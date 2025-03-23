namespace Uccs.Fair;

public class TextEntry : Text, ITableEntry
{
	public BaseId		Key => Id;
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public TextEntry()
	{
	}

	public TextEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public TextEntry Clone()
	{
		var a = new TextEntry(Mcv){	Id			= Id,
									Entities	= Entities
									};

		return a;
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

