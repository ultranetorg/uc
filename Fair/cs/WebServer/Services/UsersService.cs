using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class UsersService
(
	ILogger<UsersService> logger,
	FairMcv mcv
) : IUsersService
{
	public UserModel Find(string userId)
	{
		logger.LogDebug($"GET {nameof(UsersService)}.{nameof(UsersService.Find)} method called with {{UserId}}", userId);

		Guard.Against.NullOrEmpty(userId);

		EntityId entityId = EntityId.Parse(userId);

		FairAccount account = null;
		lock (mcv.Lock)
		{
			account = (FairAccount) mcv.Accounts.Find(entityId, mcv.LastConfirmedRound.Id);
			if (account == null)
			{
				return null;
			}
		}

		IEnumerable<UserSiteModel> sites = account.Sites?.Length > 0 ? LoadSites(account.Sites) : null;
		IEnumerable<UserAuthorModel> authors = account.Authors.Length > 0 ? LoadAuthors(account.Authors) : null;

		IEnumerable<UserProductModel> products = null;
		IEnumerable<UserPublicationModel> publications = null;
		if (account.Authors.Length > 0)
		{
			LoadProductsResult loadProductsResult = LoadProducts(account.Authors);
			products = loadProductsResult.ProductsModels;
			publications = loadProductsResult.Products?.Length > 0 ? LoadPublications(loadProductsResult.Products) : null;
		}

		return new UserModel(account.Id.ToString())
		{
			Sites = sites,
			Authors = authors,
			Publications = publications,
			Products = products,
		};
	}

	private IEnumerable<UserSiteModel> LoadSites(EntityId[] sitesIds)
	{
		lock (mcv.Lock)
		{
			return sitesIds.Select(id =>
			{
				Site site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
				return new UserSiteModel(site)
				{
					ProductsCount = 0, // TODO: calculate products count.
					Url = SiteUtils.Url(site),
				};
			}).ToArray();
		}
	}

	private IEnumerable<UserAuthorModel> LoadAuthors(EntityId[] authorsIds)
	{
		lock (mcv.Lock)
		{
			return authorsIds.Select(id =>
			{
				Author author = mcv.Authors.Find(id, mcv.LastConfirmedRound.Id);
				return new UserAuthorModel(author);
			}).ToArray();
		}
	}

	private IEnumerable<UserPublicationModel> LoadPublications(Product[] products)
	{
		lock (mcv.Lock)
		{
			return products.SelectMany(product =>
			{
				return product.Publications.Select(id =>
				{
					Publication publication = mcv.Publications.Find(id, mcv.LastConfirmedRound.Id);
					Category category = mcv.Categories.Find(publication.Category, mcv.LastConfirmedRound.Id);
					Site site = mcv.Sites.Find(category.Site, mcv.LastConfirmedRound.Id);

					return new UserPublicationModel(publication, site, category, product);
				}).ToArray();
			});
		}
	}

	private LoadProductsResult LoadProducts(EntityId[] authorsIds)
	{
		var result = new LoadProductsResult();
		var productsList = new LinkedList<Product>();
		UserProductModel[] productModels = null;

		lock (mcv.Lock)
		{
			productModels = authorsIds.SelectMany(authorId =>
			{
				AuthorEntry author = mcv.Authors.Find(authorId, mcv.LastConfirmedRound.Id);

				return author.Products.Select(productId =>
				{
					Product product = mcv.Products.Find(productId, mcv.LastConfirmedRound.Id);
					productsList.AddLast(product);
					return new UserProductModel(product);
				}).ToArray();
			}).ToArray();
		}

		result.Products = productsList.Count > 0 ? productsList.ToArray() : null;
		result.ProductsModels = productModels.Length > 0 ? productModels : null;

		return result;
	}

	private class LoadProductsResult
	{
		public IEnumerable<UserProductModel> ProductsModels { get; set; }

		public Product[] Products { get; set; }
	}
}
