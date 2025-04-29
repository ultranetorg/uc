namespace Uccs.Fair;

public enum ReviewStatus : byte
{
	None,
	Pending,
	Accepted,
	Rejected,
}

//public class ReviewVersion  : IBinarySerializable
//{
//	public EntityId		Review { get; set; }
//	public byte			Version { get; set; }
//
//	public void Read(BinaryReader reader)
//	{
//		Review = reader.Read<EntityId>();
//		Version = reader.ReadByte();
//	}
//
//	public void Write(BinaryWriter writer)
//	{
//		writer.Write(Review);
//		writer.Write(Version);
//	}
//}

public class Review : IBinarySerializable, ITableEntry
{
	public AutoId			Id { get; set; }
	public AutoId			Publication { get; set; }
    public AutoId			Creator { get; set; }
	public ReviewStatus		Status { get; set; }
	public byte				Rate { get; set; }
    public string			Text { get; set; }
    public string			TextNew { get; set; }
    public Time	    		Created { get; set; }

	public EntityId			Key => Id;
	public bool				Deleted { get; set; }
	FairMcv					Mcv;

	public Review()
	{
	}

	public Review(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public Review Clone()
	{
		return new(Mcv){Id			= Id,
						Publication = Publication,
						Creator		= Creator,
						Status		= Status,
						Rate		= Rate,
						Text		= Text,
						TextNew		= TextNew,
						Created		= Created};
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
		Publication	= reader.Read<AutoId>();
		Creator		= reader.Read<AutoId>();
		Status		= reader.Read<ReviewStatus>();
		Rate		= reader.ReadByte();
		Text		= reader.ReadString();
		TextNew		= reader.ReadString();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Publication);
		writer.Write(Creator);
		writer.Write(Status);
		writer.Write(Rate);
		writer.Write(Text);
		writer.Write(TextNew);
		writer.Write(Created);
	}
}
