using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AccountsService
(
	ILogger<AccountsService> logger,
	FairMcv mcv
)
{
	public UserModel Get([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(AccountsService), nameof(Get), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId id = AutoId.Parse(userId);

		FairAccount account = null;
		lock (mcv.Lock)
		{
			account = (FairAccount) mcv.Accounts.Find(id, mcv.LastConfirmedRound.Id);
			if (account == null)
			{
				throw new EntityNotFoundException(nameof(Account).ToLower(), userId);
			}
		}

		IEnumerable<UserSiteModel> sites = account.ModeratedSites?.Length > 0 ? LoadSites(account.ModeratedSites) : [];
		IEnumerable<UserAuthorModel> authors = account.Authors.Length > 0 ? LoadAuthors(account.Authors) : [];

		// IEnumerable<UserProductModel> products = null;
		IEnumerable<UserPublicationModel> publications = null;
		if (account.Authors.Length > 0)
		{
			LoadProductsResult loadProductsResult = LoadProducts(account.Authors);
			// products = loadProductsResult.ProductsModels;
			publications = loadProductsResult.Products?.Length > 0 ? LoadPublications(loadProductsResult.Products) : [];
		}

		return new UserModel(account.Id.ToString())
		{
			Sites = sites,
			Authors = authors,
			Publications = publications,
			// Products = products,
		};
	}

	private IEnumerable<UserSiteModel> LoadSites(AutoId[] sitesIds)
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

	private IEnumerable<UserAuthorModel> LoadAuthors(AutoId[] authorsIds)
	{
		lock (mcv.Lock)
		{
			return authorsIds.Select(id =>
			{
				Author author = mcv.Authors.Latest(id);
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

	private LoadProductsResult LoadProducts(AutoId[] authorsIds)
	{
		var result = new LoadProductsResult();
		var productsList = new LinkedList<Product>();
		UserProductModel[] productModels = null;

		lock (mcv.Lock)
		{
			productModels = authorsIds.SelectMany(authorId =>
			{
				Author author = mcv.Authors.Find(authorId, mcv.LastConfirmedRound.Id);

				return author.Products.Select(productId =>
				{
					Product product = mcv.Products.Find(productId, mcv.LastConfirmedRound.Id);
					productsList.AddLast(product);
					return new UserProductModel(product);
				}).ToArray();
			}).ToArray();
		}

		result.Products = productsList.Count > 0 ? productsList.ToArray() : [];
		result.ProductsModels = productModels.Length > 0 ? productModels : [];

		return result;
	}

	public FileContentResult GetAvatar([NotNull][NotEmpty] string accountId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {AccountId}", nameof(AccountsService), nameof(GetAvatar), accountId);

		Guard.Against.NullOrEmpty(accountId);

		AutoId id = AutoId.Parse(accountId);

		lock(mcv.Lock)
		{
			FairAccount account = (FairAccount) mcv.Accounts.Latest(id);
			if(account == null || account.Avatar == null)
			{
				throw new EntityNotFoundException(nameof(Account).ToLower(), accountId);
			}

			return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
		}
	}

	private class LoadProductsResult
	{
		public IEnumerable<UserProductModel> ProductsModels { get; set; }

		public Product[] Products { get; set; }
	}
}
