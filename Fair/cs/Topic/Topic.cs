namespace Uccs.Fair;

public class Review : IBinarySerializable
{
    public EntityId		User { get; set; }
    public string		Text { get; set; }
    public byte	    	Rating { get; set; }
    public Time	    	Created { get; set; }

	public void Read(BinaryReader reader)
	{
		User	= reader.Read<EntityId>();
		Text	= reader.ReadUtf8();
		Rating	= reader.ReadByte();
		Created	= reader.Read<Time>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(User);
		writer.Write(Text);
		writer.Write(Rating);
		writer.Write(Created);
	}
}

public class Topic
{
 	public EntityId		Id { get; set; }
 	public EntityId		Catalogue { get; set; }
 	public EntityId		Product { get; set; }
    public Review[]     Reviews { get; set; }

}
