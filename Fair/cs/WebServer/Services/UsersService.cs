using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class UsersService
(
	ILogger<UsersService> logger,
	FairMcv mcv
)
{
	public bool SiteExists([NotNull][NotEmpty] string userId, [NotNull][NotEmpty] string siteId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}, {SiteId}", nameof(UsersService), nameof(SiteExists), userId, siteId);

		Guard.Against.NullOrEmpty(userId);
		Guard.Against.NullOrEmpty(siteId);

		AutoId userEntityId = AutoId.Parse(userId);

		lock(mcv.Lock)
		{
			FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
			if(user == null)
			{
				throw new EntityNotFoundException(nameof(User), userId);
			}

			AutoId siteEntityId = AutoId.Parse(siteId);
			return user.Sites.Contains(siteEntityId);
		}
	}
}
