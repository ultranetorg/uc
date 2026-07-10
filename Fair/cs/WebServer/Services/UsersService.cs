using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersService
(
	ILogger<UsersService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<UserModel> GetSiteUsers([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(UsersService), nameof(GetSiteUsers), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId entityId = AutoId.Parse(siteId);

		Store site = mcv.Stores.Latest(entityId);
		if(site == null)
		{
			throw new EntityNotFoundException(nameof(Store), siteId);
		}

		IEnumerable<AutoId> paged = site.Users.Skip(page * pageSize).Take(pageSize);
		IEnumerable<UserModel> items = site.Users.Length > 0 ? LoadUsers(paged, cancellationToken) : [];

		return new TotalItemsResult<UserModel>
		{
			Items = items,
			TotalItems = site.Users.Length
		};
	}

	IEnumerable<UserModel> LoadUsers(IEnumerable<AutoId> usersIds, CancellationToken cancellationToken)
	{
		return usersIds.Select(id =>
		{
			cancellationToken.ThrowIfCancellationRequested();

			FairUser user = (FairUser) mcv.Users.Latest(id);
			return new UserModel(user);
		}).ToArray();
	}

	public UserModel GetUserByName([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetUserByName), name);

		Guard.Against.NullOrEmpty(name);

		FairUser user = (FairUser) mcv.Users.Latest(name);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), name);
		}

		return GetUser(user);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	UserModel GetUser(FairUser account)
	{
		return new UserModel
		{
			Id = account.Id.ToString(),
			Name = account.Name,
			Owner = account.Owner.ToString()
		};
	}

	public UserDetailsModel GetDetails([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetDetails), name);

		Guard.Against.NullOrEmpty(name);

		FairUser account = (FairUser) mcv.Users.Latest(name);
		if(account == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new UserDetailsModel
		{
			Id = account.Id.ToString(),
			Name = account.Name,
			Owner = account.Owner.ToString(),
			AuthorsIds = account.Authors.Select(id => id.ToString()),
			FavoriteSites = account.FavoriteStores.Length > 0 ? LoadAccountSites(account.FavoriteStores) : []
		};
	}

	IEnumerable<SiteBaseModel> LoadAccountSites(AutoId[] sitesIds)
	{
		return sitesIds.Select(id =>
		{
			Store site = mcv.Stores.Latest(id);
			return new SiteBaseModel(site);
		}).ToArray();
	}

	public UserAuthorsModel GetUserAuthors([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetUserAuthors), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		return new UserAuthorsModel
		{
			Id = user.Id.ToString(),
			Name = user.Name,
			Owner = user.Owner.ToString(),
			Authors = user.Authors.Length != 0 ? LoadAuthors(user.Authors) : []
		};
	}

	IEnumerable<AuthorBaseAvatarModel> LoadAuthors(AutoId[] authorsIds)
	{
		return authorsIds.Select(id =>
		{
			Author author = mcv.Authors.Latest(id);
			return new AuthorBaseAvatarModel(author);
		}).ToArray();
	}

	public bool SiteExists([NotNull][NotEmpty] string userId, [NotNull][NotEmpty] string siteId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}, {SiteId}", nameof(UsersService), nameof(SiteExists), userId, siteId);

		Guard.Against.NullOrEmpty(userId);
		Guard.Against.NullOrEmpty(siteId);

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		AutoId siteEntityId = AutoId.Parse(siteId);
		return user.Stores.Contains(siteEntityId);
	}

	public FileContentResult GetAvatarById([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetAvatarById), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId entityId = AutoId.Parse(userId);

		FairUser account = (FairUser) mcv.Users.Latest(entityId);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), userId);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}

	public FileContentResult GetAvatarByName([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetAvatarById), name);

		Guard.Against.NullOrEmpty(name);

		FairUser account = (FairUser)mcv.Users.Latest(name);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}
}
