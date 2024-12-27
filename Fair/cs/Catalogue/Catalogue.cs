
namespace Uccs.Fair;

public class Catalogue// : IBinarySerializable
{
	public EntityId		Id { get; set; }
	public string		Title { get; set; }
	public EntityId[]	Owners { get; set; }
	public EntityId[]	Topics { get; set; }
}
