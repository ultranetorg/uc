namespace Uccs.Fair;

public class StoreModel(Store store) : StoreBaseModel(store)
{
	public IEnumerable<AutoId> ModeratorsIds { get; set; }
	public IEnumerable<AutoId> AuthorsIds { get; set; }

	public bool HasPublications { get; init; }
}
