using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class AuthorsService
(
	ILogger<AuthorsService> logger,
	FairMcv mcv
) : IAuthorsService
{
	public AuthorModel Find(string authorId)
	{
		logger.LogDebug($"GET {nameof(AuthorsService)}.{nameof(AuthorsService.Find)} method called with {{AuthorId}}", authorId);

		Guard.Against.NullOrEmpty(authorId);

		EntityId entityId = EntityId.Parse(authorId);

		Author author = null;
		lock (mcv.Lock)
		{
			author = mcv.Authors.Find(entityId, mcv.LastConfirmedRound.Id);
			if (author == null)
			{
				return null;
			}
		}

		return ToAuthorModel(author);
	}

	private static AuthorModel ToAuthorModel(Author author)
	{
		return new AuthorModel(author)
		{
			OwnerId = author.Owner.ToString(),
		};
	}
}
