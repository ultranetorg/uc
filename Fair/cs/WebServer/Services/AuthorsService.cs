using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class AuthorsService
(
	ILogger<AuthorsService> logger,
	FairMcv mcv
): IAuthorsService
{
	public AuthorDetailsModel GetDetails(string authorId)
	{
		logger.LogDebug($"GET {nameof(AuthorsService)}.{nameof(AuthorsService.GetDetails)} method called with {{AuthorId}}", authorId);

		Guard.Against.NullOrEmpty(authorId);

		AutoId authorEntityId = AutoId.Parse(authorId);

		lock (mcv.Lock)
		{
			Author author = mcv.Authors.Find(authorEntityId, mcv.LastConfirmedRound.Id);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			byte[]? avatar = author.Avatar != null ? mcv.Files.Latest(author.Avatar).Data : null;

			return new AuthorDetailsModel(author, avatar);
		}
	}
}
