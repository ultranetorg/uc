using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class AuthorsService
(
#if DEBUG
	ILogger<AuthorsService> logger,
#endif
	FairMcv mcv
)
{
	public AuthorDetailsModel GetDetails(string authorId)
	{
		ArgumentException.ThrowIfNullOrEmpty(authorId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {AuthorId}", nameof(AuthorsService), nameof(AuthorsService.GetDetails), authorId);
#endif

		AutoId authorEntityId = AutoId.Parse(authorId);

		Author author = mcv.Authors.Latest(authorEntityId);
		if (author == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return new AuthorDetailsModel(author)
		{
			Description = author.Description,
			AvatarId = author.Avatar,
			OwnersIds = LoadOwners(author.Owners)
		};
	}

	IEnumerable<UserModel> LoadOwners(IEnumerable<AutoId> ownersIds)
	{
		return ownersIds.Select(x =>
		{
			FairUser user = (FairUser)mcv.Users.Latest(x);
			return new UserModel(user);
		}).ToArray();
	}

	public TotalItemsResult<ProductAuthorModel> GetProducts([NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(authorId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {AuthorId}, {Page}, {PageSize}", nameof(AuthorsService), nameof(AuthorsService.GetProducts), authorId, page, pageSize);
#endif

		AutoId authorEntityId = AutoId.Parse(authorId);
		Author author = mcv.Authors.Latest(authorEntityId);
		if(author == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		var items = new List<ProductAuthorModel>(pageSize);
		var pagedProducts = author.Products.Skip(page * pageSize).Take(pageSize);
		LoadProducts(items, pagedProducts, cancellationToken);

		return new TotalItemsResult<ProductAuthorModel>
		{
			TotalItems = author.Products.Length,
			Items = items,
		};
	}

	void LoadProducts(List<ProductAuthorModel> items, IEnumerable<AutoId> productsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested) return;

		foreach(var productId in productsIds)
		{
			if(cancellationToken.IsCancellationRequested) return;

			Product product = mcv.Products.Latest(productId);
			ProductAuthorModel model = new ProductAuthorModel(product)
			{
				Id = product.Id,
				Title = PublicationUtils.GetLatestTitle(product),
				LogoId = PublicationUtils.GetLatestLogo(product),
				PublicationsCount = product.Publications.Length
			};
			items.Add(model);
		}
	}
}
