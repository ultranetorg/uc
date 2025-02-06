namespace Uccs.Fair;

public enum ReviewStatus : byte
{
	None,
	Pending,
	Accepted,
	Rejected,
	Disputed
}

public class Review : IBinarySerializable
{
	public EntityId						Id { get; set; }
	public EntityId						Publication { get; set; }
    public EntityId						User { get; set; }
	public ReviewStatus					Status { get; set; }
    public string						Text { get; set; }
    public byte							Rating { get; set; }
    public Time	    					Created { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Publication	= reader.Read<EntityId>();
		User		= reader.Read<EntityId>();
		Status		= reader.ReadEnum<ReviewStatus>();
		Text		= reader.ReadUtf8();
		Rating		= reader.ReadByte();
		Created		= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Publication);
		writer.Write(User);
		writer.WriteEnum(Status);
		writer.Write(Text);
		writer.Write(Rating);
		writer.Write(Created);
	}
}
