namespace Uccs.Fair;

public class Comment : IBinarySerializable
{
	public EntityId		Id { get; set; }
    public EntityId		User { get; set; }
    public string		Text { get; set; }
    public byte			Rating { get; set; }
    public Time	    	Created { get; set; }

	public void Read(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		User	= reader.Read<EntityId>();
		Text	= reader.ReadUtf8();
		Rating	= reader.ReadByte();
		Created	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(User);
		writer.Write(Text);
		writer.Write(Rating);
		writer.Write(Created);
	}
}
