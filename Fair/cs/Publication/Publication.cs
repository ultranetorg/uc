namespace Uccs.Fair;

public enum PublicationStatus : byte
{
	None,
	RequestedByAuthor,
	ProposedBySite,
	Active,
	//Disputed
}

public class Publication : IBinarySerializable
{
	public EntityId					Id { get; set; }
	public EntityId					Category { get; set; }
	public EntityId					Creator { get; set; }
	public EntityId					Product { get; set; }
	public PublicationStatus		Status { get; set; }
	public ProductFieldVersionId[]	Fields { get; set; }
	public ProductFieldVersionId[]	Changes { get; set; }
	public EntityId[]				Reviews { get; set; }
	public EntityId[]				ReviewChanges { get; set; }

	public void Read(BinaryReader reader)
	{
		Id				= reader.Read<EntityId>();
		Category		= reader.Read<EntityId>();
		Creator			= reader.Read<EntityId>();
		Product			= reader.Read<EntityId>();
		Status			= reader.ReadEnum<PublicationStatus>();
		Fields			= reader.ReadArray<ProductFieldVersionId>();
		Changes			= reader.ReadArray<ProductFieldVersionId>();
		Reviews			= reader.ReadArray<EntityId>();
		ReviewChanges	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Category);
		writer.Write(Creator);
		writer.Write(Product);
		writer.WriteEnum(Status);
		writer.Write(Fields);
		writer.Write(Changes);
		writer.Write(Reviews);
		writer.Write(ReviewChanges);
	}
}
