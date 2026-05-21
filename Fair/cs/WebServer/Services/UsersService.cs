using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Runtime.CompilerServices;
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
	public UserModel GetUserByName([NotNull][NotEmpty] string name)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(GetUserByName), name);

		Guard.Against.NullOrEmpty(name);

		lock(mcv.Lock)
		{
			FairUser user = (FairUser) mcv.Users.Latest(name);
			if(user == null)
			{
				throw new EntityNotFoundException(nameof(User), name);
			}

			return GetUser(user);
		}
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

		lock(mcv.Lock)
		{
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
				FavoriteSites = account.FavoriteSites.Length > 0 ? LoadAccountSites(account.FavoriteSites) : []
			};
		}
	}

	IEnumerable<SiteBaseModel> LoadAccountSites(AutoId[] sitesIds)
	{
		return sitesIds.Select(id =>
		{
			Site site = mcv.Sites.Latest(id);
			return new SiteBaseModel(site);
		}).ToArray();
	}

	public UserAuthorsModel GetUserAuthors([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetUserAuthors), userId);

		Guard.Against.NullOrEmpty(userId);

		AutoId userEntityId = AutoId.Parse(userId);

		lock(mcv.Lock)
		{
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

	public FileContentResult GetAvatar([NotNull][NotEmpty] string userId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(GetAvatar), userId);

		Guard.Against.NullOrEmpty(userId);

		lock(mcv.Lock)
		{
			FairUser account = (FairUser) mcv.Users.Latest(userId);
			if(account == null || account.Avatar == null)
			{
				throw new EntityNotFoundException(nameof(User).ToLower(), userId);
			}

			return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
		}
	}

	//public UserModel GetUserById([NotNull][NotEmpty] string id)
	//{
	//	logger.LogDebug("{ClassName}.{MethodName} method called with {Id}", nameof(UsersService), nameof(GetUserById), id);

	//	Guard.Against.NullOrEmpty(id);

	//	AutoId entityId = AutoId.Parse(id);

	//	lock(mcv.Lock)
	//	{
	//		FairUser user = (FairUser) mcv.Users.Latest(entityId);
	//		if(user == null)
	//		{
	//			throw new EntityNotFoundException(nameof(User), id);
	//		}

	//		return GetUser(user);
	//	}
	//}
}
