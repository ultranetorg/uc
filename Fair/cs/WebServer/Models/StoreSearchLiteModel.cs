namespace Uccs.Fair;

public sealed class StoreSearchLiteModel
{
	public AutoId Id { get; set; }

	public string Title { get; set; }

	public StoreSearchLiteModel(Store store)
	{
		Id = store.Id;
		Title = store.Title;
	}

	public StoreSearchLiteModel(AutoId id, string title)
	{
		Id = id;
		Title = title;
	}
}
