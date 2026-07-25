namespace Uccs.Fair; 

public class UserStoreModel(Store store) : StoreBaseModel(store)
{
	public int ProductsCount { get; init; }

	public string Url { get; init; }
}
