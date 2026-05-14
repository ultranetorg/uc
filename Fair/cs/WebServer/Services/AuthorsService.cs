using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class AuthorsService
(
	ILogger<AuthorsService> logger,
	FairMcv mcv
)
{
	public AuthorDetailsModel GetDetails(string authorId)
	{
		logger.LogDebug($"GET {nameof(AuthorsService)}.{nameof(AuthorsService.GetDetails)} method called with {{AuthorId}}", authorId);

		Guard.Against.NullOrEmpty(authorId);

		AutoId authorEntityId = AutoId.Parse(authorId);

		lock (mcv.Lock)
		{
			Author author = mcv.Authors.Latest(authorEntityId);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			return new AuthorDetailsModel(author)
			{
				Description = author.Description,
				AvatarId = author.Avatar?.ToString(),
				Links = author.Links.Select(x => x.ToString()),
				OwnersIds = LoadOwners(author.Owners)
			};
		}
	}

	IEnumerable<UserModel> LoadOwners(IEnumerable<AutoId> ownersIds)
	{
		return ownersIds.Select(x =>
		{
			FairUser user = (FairUser)mcv.Users.Latest(x);
			return new UserModel(user);
		}).ToArray();
	}
}
