using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class AuthorsService
(
	ILogger<AuthorsService> logger,
	FairMcv mcv
): IAuthorsService
{
	public SiteAuthorModel GetAuthor(string authorId)
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

			return new SiteAuthorModel(author);
		}
	}
}
