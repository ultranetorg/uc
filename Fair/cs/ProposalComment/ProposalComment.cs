namespace Uccs.Fair;

public class ProposalComment : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Proposal { get; set; }
    public AutoId			Creator { get; set; }
    public string			Text { get; set; }
    public Time	    		Created { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public ProposalComment()
	{
	}

	public ProposalComment(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return new ProposalComment(Mcv) {Id			= Id,
										Proposal	= Proposal,
										Creator		= Creator,
										Text		= Text,
										Created		= Created
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
		Id			= reader.Read<AutoId>();
		Proposal		= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
		Text		= reader.ReadUtf8();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Proposal);
		writer.Write(Creator);
		writer.WriteUtf8(Text);
		writer.Write(Created);
	}
}
