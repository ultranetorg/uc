namespace Uccs.Fair;

public class DisputeComment : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Dispute { get; set; }
    public AutoId			Creator { get; set; }
    public string			Text { get; set; }
    public Time	    		Created { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public DisputeComment()
	{
	}

	public DisputeComment(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public object Clone()
	{
		return new DisputeComment(Mcv) {Id			= Id,
										Dispute		= Dispute,
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
		Dispute		= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
		Text		= reader.ReadUtf8();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Dispute);
		writer.Write(Creator);
		writer.WriteUtf8(Text);
		writer.Write(Created);
	}
}
