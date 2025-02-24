namespace Uccs.Fair;

public class DisputeEntry : Dispute, ITableEntry
{
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
										Proposal	= Proposal,
										Pros		= Pros,
										Cons		= Cons,
										Expirtaion	= Expirtaion
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

