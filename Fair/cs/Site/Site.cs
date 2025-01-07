
namespace Uccs.Fair;

public class Site : IBinarySerializable
{
	public EntityId		Id { get; set; }
	public string		Title { get; set; }
	public EntityId[]	Owners { get; set; }
	public EntityId[]	Roots { get; set; }


	public void Read(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Title	= reader.ReadUtf8();
		Owners	= reader.ReadArray<EntityId>();
		Roots	= reader.ReadArray<EntityId>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Title);
		writer.Write(Owners);
		writer.Write(Roots);
	}

}
