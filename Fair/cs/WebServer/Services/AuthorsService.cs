using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class AuthorsService
(
	ILogger<AuthorsService> logger,
	FairMcv mcv
): IAuthorsService
{
	public AuthorModel GetAuthor(string authorId)
	{
		logger.LogDebug($"GET {nameof(AuthorsService)}.{nameof(AuthorsService.GetAuthor)} method called with {{AuthorId}}", authorId);

		Guard.Against.NullOrEmpty(authorId);

		EntityId authorEntitiId = EntityId.Parse(authorId);

		lock (mcv.Lock)
		{
			Author author = mcv.Authors.Find(authorEntitiId, mcv.LastConfirmedRound.Id);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			return new AuthorModel(author);
		}
	}

	//public TotalItemsResult<AuthorBaseModel> GetAuthors(string siteId, int page, int pageSize)
	//{
	//	logger.LogDebug($"GET {nameof(SitesService)}.{nameof(SitesService.GetAuthors)} method called with {{SiteId}}, {{Page}}, {{PageSize}}", siteId, page, pageSize);

	//	Guard.Against.NullOrEmpty(siteId);

	//	EntityId id = EntityId.Parse(siteId);

	//	Site site = null;
	//	lock (mcv.Lock)
	//	{
	//		site = mcv.Sites.Find(id, mcv.LastConfirmedRound.Id);
	//		if (site == null)
	//		{
	//			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
	//		}
	//	}

	//	IEnumerable<EntityId> authorsIds = site.Authors.Skip(page * pageSize).Take(pageSize);
	//	IEnumerable<AuthorBaseModel> items = authorsIds.Count() > 0 ? LoadAuthors(authorsIds) : null;

	//	return new TotalItemsResult<AuthorBaseModel>
	//	{
	//		Items = items,
	//		TotalItems = site.Authors.Count(),
	//	};
	//}

	//private IEnumerable<AuthorBaseModel> LoadAuthors(IEnumerable<EntityId> authorsIds)
	//{
	//	lock (mcv.Lock)
	//	{
	//		return authorsIds.Select(id =>
	//		{
	//			Author account = mcv.Authors.Find(id, mcv.LastConfirmedRound.Id);
	//			return new AuthorBaseModel(account);
	//		}).ToArray();
	//	}
	//}
}
