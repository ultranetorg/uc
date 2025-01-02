
namespace Uccs.Fair;

public class Site// : IBinarySerializable
{
	public EntityId		Id { get; set; }
	public string		Title { get; set; }
	public EntityId[]	Owners { get; set; }
	public EntityId[]	Roots { get; set; }
}
