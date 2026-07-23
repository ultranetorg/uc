namespace Uccs.Fair;

public class StoreModel(Store store) : StoreBaseModel(store)
{
	public IEnumerable<string> ModeratorsIds { get; set; }
	public IEnumerable<string> AuthorsIds { get; set; }
}
