using Ardalis.GuardClauses;

namespace Uccs.Smp;

public class UsersService
(
	ILogger<UsersService> logger,
	SmpMcv mcv
) : IUsersService
{
	public UserModel Find(string userId)
	{
		logger.LogDebug($"GET {nameof(UsersService)}.{nameof(UsersService.Find)} method called with {{UserId}}", userId);

		Guard.Against.NullOrEmpty(userId);

		EntityId entityId = EntityId.Parse(userId);

		SmpAccountEntry account = null;
		lock (mcv.Lock)
		{
			account = (SmpAccountEntry) mcv.Accounts.Find(entityId, mcv.LastConfirmedRound.Id);
			if (account == null)
			{
				return null;
			}
		}

		AuthorBaseModel[] authors = account.Authors.Length > 0 ? LoadAuthors(account.Authors) : null;
		// UserPublicationModel[] publications = account.Pu

		return ToUserModel(account, authors);
	}

	private AuthorBaseModel[] LoadAuthors(EntityId[] authorsIds)
	{
		lock (mcv.Lock)
		{
			return authorsIds.Select(id =>
			{
				Author author = mcv.Authors.Find(id, mcv.LastConfirmedRound.Id);
				return new AuthorBaseModel(author);
			}).ToArray();
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

	private static UserModel ToUserModel(SmpAccountEntry account, AuthorBaseModel[] authors)
	{
		return new UserModel
		{
			Id = account.Id.ToString(),

			Authors = authors
		};
	}
}
