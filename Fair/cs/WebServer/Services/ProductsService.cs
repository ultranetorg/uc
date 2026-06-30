using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsService
(
	ILogger<ProductsService> logger,
	FairMcv mcv
)
{
	public IEnumerable<FieldValueModel>? GetFields([NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(GetFields), productId);

		Guard.Against.NullOrEmpty(productId);

		AutoId id = AutoId.Parse(productId);

		Product product = mcv.Products.Latest(id);
		if(product == null)
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		return ProductFieldsUtils.GetLatestMappedFields(product);
	}

	public ProductDetailsModel GetDetails([NotNull][NotEmpty] string productId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(GetDetails), productId);

		Guard.Against.NullOrEmpty(productId);

		AutoId id = AutoId.Parse(productId);

		Product product = mcv.Products.Latest(id);
		if(product == null)
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		Author author = mcv.Authors.Latest(product.Author);

		IEnumerable<FieldValueModel>? productFields = ProductFieldsUtils.GetLatestMappedFields(product);

		return new ProductDetailsModel
		{
			Id = product.Id.ToString(),
			Type = product.Type,
			Title = PublicationUtils.GetLatestTitle(product),
			LogoId = PublicationUtils.GetLatestLogo(product)?.ToString(),
			Updated = product.Updated.Hours,
			Fields = productFields,
			AuthorId = author.Id.ToString(),
			AuthorTitle = author.Title,
			AuthorLogoId = author.Avatar?.ToString()
		};
	}

	public PublicationDetailsDiffModel GetDiff([NotNull][NotEmpty] string publicationId, [NonNegativeValue] int version)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(ProductsService), nameof(GetDiff), publicationId);

		Guard.Against.NullOrEmpty(publicationId);
		Guard.Against.Negative(version);

		AutoId id = AutoId.Parse(publicationId);

		Publication publication = mcv.Publications.Latest(id);
		if(publication == null)
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		Product product = mcv.Products.Latest(publication.Product);
		if(product.Versions.Length < 1 || product.Versions.All(x => x.Id != version))
		{
			throw new InvalidPublicationVersionException(publicationId, version);
		}

		Author author = mcv.Authors.Latest(product.Author);
		Category category = mcv.Categories.Latest(publication.Category);

		var fields = ProductFieldsUtils.GetMappedFieldsVersion(product, publication.ProductVersion);
		var fieldsTo = ProductFieldsUtils.GetMappedFieldsVersion(product, version);

		return new PublicationDetailsDiffModel
		{
			Id = publication.Id.ToString(),
			Type = product.Type,
			Title = PublicationUtils.GetTitle(publication, product),
			LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
			Updated = product.Updated.Hours,
			Fields = fields,
			AuthorId = author.Id.ToString(),
			AuthorTitle = author.Title,
			AuthorLogoId = author.Avatar?.ToString(),
			CategoryId = category?.Id.ToString(),
			CategoryTitle = category?.Title,
			Rating = publication.Rating,
			FieldsTo = fieldsTo
		};
	}

	public TotalItemsResult<ProductStoreModel> GetProductStores([NotNull][NotEmpty] string productId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}, {Page}, {PageSize}", nameof(ProductsService), nameof(GetProductStores), productId, page, pageSize);

		Guard.Against.NullOrEmpty(productId);
		Guard.Against.Negative(page);
		Guard.Against.Negative(pageSize);

		AutoId id = AutoId.Parse(productId);

		Product product = mcv.Products.Latest(id);
		if(product == null)
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		IEnumerable<AutoId> publicationsIds = product.Publications.Skip(page * pageSize).Take(pageSize);
		return LoadProductStores(publicationsIds, product.Publications.Length, cancellationToken);
	}

	TotalItemsResult<ProductStoreModel> LoadProductStores(IEnumerable<AutoId> publicationsIds, int totalItems, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProductStoreModel>.Empty;

		var result = new List<ProductStoreModel>(publicationsIds.Count());

		foreach(var publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<ProductStoreModel> {Items = result, TotalItems = totalItems};

			Publication publication = mcv.Publications.Latest(publicationId);
			Site store = mcv.Sites.Latest(publication.Site);

			var model = new ProductStoreModel
			{
				Id = store.Id.ToString(),
				Title = store.Title,
				AvatarId = store.Avatar?.ToString()
			};
			result.Add(model);
		}

		return new TotalItemsResult<ProductStoreModel>
		{
			Items = result,
			TotalItems = totalItems
		};
	}

	public IEnumerable<ProductSearchResultBaseModel> SearchLite([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(ProductsService), nameof(ProductsService.SearchLite), query, limit);

		Guard.Against.NullOrEmpty(query);
		Guard.Against.NegativeOrZero(limit);

		var result = mcv.ProductTitles.Search(query, 0, limit);
		return MapTo<ProductSearchResultBaseModel>(result, cancellationToken);
	}

	public IEnumerable<ProductSearchResultModel> Search([NotNull][NotEmpty] string query, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Page}, {PageSize}", nameof(ProductsService), nameof(ProductsService.Search), query, page, pageSize);

		Guard.Against.NullOrEmpty(query);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		var result = mcv.ProductTitles.Search(query, page * pageSize, pageSize);
		return MapTo<ProductSearchResultModel>(result, cancellationToken);
	}

	IEnumerable<T> MapTo<T>(IList<ProductSearchResult> results, CancellationToken cancellationToken) where T : ProductSearchResultBaseModel, new()
	{
		foreach (var item in results)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			var model = new T
			{
				ProductId = item.Product.ToString(),
				ProductTitle = item.ProductTitle,
				AuthorId = item.Author.ToString(),
				AuthorTitle = item.AuthorTitle,
				PublicationId = item.Publications[0].ToString(),
				AvatarId = item.Avatar?.ToString(),
			};

			if (model is ProductSearchResultModel full)
				full.SitesPublications = LoadSitePublications(item.Publications, cancellationToken).ToArray();

			yield return model;
		}
	}

	IEnumerable<SitePublicationModel> LoadSitePublications(IEnumerable<AutoId> publicationsIds, CancellationToken cancellationToken)
	{
		foreach(var publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;


			Publication publication = mcv.Publications.Latest(publicationId);
			Site site = mcv.Sites.Latest(publication.Site);
			yield return new SitePublicationModel
			{
				SiteId = site.Id.ToString(),
				SiteTitle = site.Title,
				PublicationId = publication.Id.ToString()
			};
		}
	}
}