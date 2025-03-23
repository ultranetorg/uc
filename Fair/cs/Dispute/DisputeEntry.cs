namespace Uccs.Fair;

public class DisputeEntry : Dispute, ITableEntry
{
	public BaseId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;
		
	public DisputeEntry()
	{
	}

	public DisputeEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public DisputeEntry Clone()
	{
		var a = new DisputeEntry(Mcv){	
										Id			= Id,	
										Site		= Site,	
										Flags		= Flags,
										Yes			= Yes,
										No			= No,
										Abs			= Abs,
										Expirtaion	= Expirtaion,
										Text		= Text,
										Proposal	= Proposal,
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

