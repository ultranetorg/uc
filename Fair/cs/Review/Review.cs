namespace Uccs.Fair;

public enum ReviewStatus : byte
{
	None,
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
	public byte				Rating { get; set; }
    public string			Text { get; set; }
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

	public object Clone()
	{
		return new Review(Mcv) {Id			= Id,
								Publication = Publication,
								Creator		= Creator,
								Status		= Status,
								Rating		= Rating,
								Text		= Text,
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
		Rating		= reader.ReadByte();
		Text		= reader.ReadString();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Publication);
		writer.Write(Creator);
		writer.Write(Status);
		writer.Write(Rating);
		writer.Write(Text);
		writer.Write(Created);
	}
}
