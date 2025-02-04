using Ardalis.GuardClauses;
using Uccs.Fair;

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

		FairAccountEntry account = null;
		lock (mcv.Lock)
		{
			account = (FairAccountEntry) mcv.Accounts.Find(entityId, mcv.LastConfirmedRound.Id);
			if (account == null)
			{
				return null;
			}
		}

		IEnumerable<UserSiteModel> sites = account.Sites.Length > 0 ? LoadSites(account.Sites) : null;
		IEnumerable<AuthorBaseModel> authors = account.Authors.Length > 0 ? LoadAuthors(account.Authors) : null;
		UserPublicationModel[] publications = null; // authors
		UserProductModel[] product = null;

		return new UserModel(account.Id.ToString())
		{
			Sites = sites,
			Authors = authors,
			Publications = publications,
			Products = product
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
					Url = "TEMPORARY URL"
				};
			});
		}
	}

	private IEnumerable<AuthorBaseModel> LoadAuthors(EntityId[] authorsIds)
	{
		lock (mcv.Lock)
		{
			return authorsIds.Select(id =>
			{
				Author author = mcv.Authors.Find(id, mcv.LastConfirmedRound.Id);
				return new AuthorBaseModel(author);
			});
		}
	}

	private UserPublicationModel[] LoadPublications(EntityId[] publicationsIds)
	{
		lock (mcv.Lock)
		{
			return publicationsIds.Select(id =>
			{
				Publication publication = mcv.Publications.Find(id, mcv.LastConfirmedRound.Id);
				Category category = mcv.Categories.Find(publication.Category, mcv.LastConfirmedRound.Id);
				Site site = mcv.Sites.Find(category.Site, mcv.LastConfirmedRound.Id);
				Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
				string publicationTitle = ProductUtils.GetTitle(product);
				return new UserPublicationModel(publication, publicationTitle, site, category.Title);
			}).ToArray();
		}
	}
}
