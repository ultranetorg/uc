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

public class Review : IBinarySerializable
{
	public EntityId			Id { get; set; }
	public EntityId			Publication { get; set; }
    public EntityId			Creator { get; set; }
	public ReviewStatus		Status { get; set; }
	public byte				Rate { get; set; }
    public string			Text { get; set; }
    public string			TextNew { get; set; }
    public byte				Rating { get; set; }
    public Time	    		Created { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Publication	= reader.Read<EntityId>();
		Creator		= reader.Read<EntityId>();
		Status		= reader.ReadEnum<ReviewStatus>();
		Rate		= reader.ReadByte();
		Text		= reader.ReadString();
		TextNew		= reader.ReadString();
		Rating		= reader.ReadByte();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Publication);
		writer.Write(Creator);
		writer.WriteEnum(Status);
		writer.Write(Rate);
		writer.Write(Text);
		writer.Write(TextNew);
		writer.Write(Rating);
		writer.Write(Created);
	}
}
