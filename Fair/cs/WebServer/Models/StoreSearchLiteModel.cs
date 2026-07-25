namespace Uccs.Fair;

public sealed class StoreSearchLiteModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public StoreSearchLiteModel(Store store)
	{
		Id = store.Id.ToString();
		Title = store.Title;
	}

	public StoreSearchLiteModel(string id, string title)
	{
		Id = id;
		Title = title;
	}
}
