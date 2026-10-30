using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class UsersService
(
#if DEBUG
	ILogger<UsersService> logger,
#endif
	FairMcv mcv
)
{
	public UserModel GetByName([NotNull][NotEmpty] string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(UsersService.GetByName), name);
#endif

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
			Id = account.Id,
			Name = account.Name,
			Owner = account.Key.ToString()
		};
	}

	public UserDetailsModel GetDetails([NotNull][NotEmpty] string name)
	{
		ArgumentException.ThrowIfNullOrEmpty(name);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(UsersService.GetDetails), name);
#endif

		FairUser account = (FairUser) mcv.Users.Latest(name);
		if(account == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new UserDetailsModel
		{
			Id = account.Id,
			Name = account.Name,
			Owner = account.Key.ToString(),
			AuthorsIds = account.Authors,
			FavoriteStores = account.FavoriteStores.Count > 0 ? LoadUserStores(account.FavoriteStores) : [],
			HasAvatar = account.Avatar != null
		};
	}

	IEnumerable<StoreBaseModel> LoadUserStores(IEnumerable<AutoId> storesIds)
	{
		return storesIds.Select(id =>
		{
			Store store = mcv.Stores.Latest(id);
			return new StoreBaseModel(store);
		}).ToArray();
	}

	public UserAuthorsModel GetAuthors([NotNull][NotEmpty] string userId)
	{
		ArgumentException.ThrowIfNullOrEmpty(userId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(UsersService.GetAuthors), userId);
#endif

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		return new UserAuthorsModel
		{
			Id = user.Id,
			Name = user.Name,
			Owner = user.Key.ToString(),
			Authors = user.Authors.Count != 0 ? LoadAuthors(user.Authors) : []
		};
	}

	IEnumerable<AuthorBaseAvatarModel> LoadAuthors(IEnumerable<AutoId> authorsIds)
	{
		return authorsIds.Select(id =>
		{
			Author author = mcv.Authors.Latest(id);
			return new AuthorBaseAvatarModel(author);
		}).ToArray();
	}

	public bool StoreExists([NotNull][NotEmpty] string userId, [NotNull][NotEmpty] string storeId)
	{
		ArgumentException.ThrowIfNullOrEmpty(userId);
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}, {StoreId}", nameof(UsersService), nameof(UsersService.StoreExists), userId, storeId);
#endif

		AutoId userEntityId = AutoId.Parse(userId);

		FairUser user = (FairUser) mcv.Users.Latest(userEntityId);
		if(user == null)
		{
			throw new EntityNotFoundException(nameof(User), userId);
		}

		AutoId storeEntityId = AutoId.Parse(storeId);
		return user.Stores.Contains(storeEntityId);
	}

	public FileContentResult GetAvatarById([NotNull][NotEmpty] string userId)
	{
		ArgumentException.ThrowIfNullOrEmpty(userId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {UserId}", nameof(UsersService), nameof(UsersService.GetAvatarById), userId);
#endif

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
		ArgumentException.ThrowIfNullOrEmpty(name);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {Name}", nameof(UsersService), nameof(UsersService.GetAvatarByName), name);
#endif

		FairUser account = (FairUser)mcv.Users.Latest(name);
		if(account == null || account.Avatar == null)
		{
			throw new EntityNotFoundException(nameof(User).ToLower(), name);
		}

		return new FileContentResult(account.Avatar, MediaTypeNames.Image.Png);
	}
}
