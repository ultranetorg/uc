namespace Uccs.Smp;

public enum PublicationStatus : byte
{
	None,
	RequestedByAuthor,
	ProposedByStore,
	Active,
	Disputed
}

public class Publication : IBinarySerializable
{
	public EntityId				Id { get; set; }
	public EntityId				Category { get; set; }
	public EntityId				Creator { get; set; }
	public EntityId				Product { get; set; }
	public PublicationStatus	Status { get; set; }
	public string[]				Sections { get; set; }
	public EntityId[]			Reviews { get; set; }

	public void Read(BinaryReader reader)
	{
		Id			= reader.Read<EntityId>();
		Category	= reader.Read<EntityId>();
		Creator		= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
		Status		= reader.ReadEnum<PublicationStatus>();
		Sections	= reader.ReadArray(reader.ReadUtf8);
		Reviews		= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Category);
		writer.Write(Creator);
		writer.Write(Product);
		writer.WriteEnum(Status);
		writer.Write(Sections, writer.WriteUtf8);
		writer.Write(Reviews);
	}
}
