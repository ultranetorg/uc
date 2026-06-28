using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

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

		Author author = mcv.Authors.Latest(authorEntityId);
		if (author == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return new AuthorDetailsModel(author)
		{
			Description = author.Description,
			AvatarId = author.Avatar?.ToString(),
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
		logger.LogDebug("{ClassName}.{MethodName} method called with {AuthorId}, {Page}, {PageSize}", nameof(AuthorsService), nameof(AuthorsService.GetProducts), authorId, page, pageSize);

		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

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
				Id = product.Id.ToString(),
				Title = PublicationUtils.GetLatestTitle(product),
				LogoId = PublicationUtils.GetLatestLogo(product)?.ToString(),
				PublicationsCount = product.Publications.Length
			};
			items.Add(model);
		}
	}
}
