using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ProductsService
(
#if DEBUG
	ILogger<ProductsService> logger,
#endif
	FairMcv mcv
)
{
	public IEnumerable<FieldValueModel>? GetFields([NotNull][NotEmpty] string productId)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(productId);

		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(ProductsService.GetFields), productId);
#endif

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
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(productId);

		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}", nameof(ProductsService), nameof(ProductsService.GetDetails), productId);
#endif

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
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(publicationId);
		ArgumentOutOfRangeException.ThrowIfNegative(version);

		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}, {Version}", nameof(ProductsService), nameof(ProductsService.GetDiff), publicationId, version);
#endif

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

		IEnumerable<CategoryPathItem>? path = category != null ? PublicationUtils.BuildPath(mcv, category).Reverse() : null;

		return new PublicationDetailsDiffModel
		{
			Id = publication.Id.ToString(),
			Type = product.Type,
			Title = product.Title,
			LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
			Updated = product.Updated.Hours,
			Fields = fields,
			AuthorId = author.Id.ToString(),
			AuthorTitle = author.Title,
			AuthorLogoId = author.Avatar?.ToString(),
			Path = path,
			Rating = publication.Rating,
			FieldsTo = fieldsTo
		};
	}

	public TotalItemsResult<ProductStoreModel> GetProductStores([NotNull][NotEmpty] string productId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(productId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegative(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}, {Page}, {PageSize}", nameof(ProductsService), nameof(ProductsService.GetProductStores), productId, page, pageSize);
#endif

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
			Store store = mcv.Stores.Latest(publication.Store);

			var model = new ProductStoreModel
			{
				StoreId = store.Id.ToString(),
				PublicationId = publication.Id.ToString(),
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

	public IEnumerable<ProductSearchResultModel> Search([NotNull][NotEmpty] string query, ProductType productType, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(query);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {ProductType}, {Page}, {PageSize}", nameof(ProductsService), nameof(ProductsService.Search), query, productType, page, pageSize);
#endif

		var result = mcv.ProductTitles.Search(query, productType, page * pageSize, pageSize);
		return MapTo(result, cancellationToken);
	}

	IEnumerable<ProductSearchResultModel> MapTo(IList<ProductSearchResult> results, CancellationToken cancellationToken)
	{
		foreach (var item in results)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			yield return new ProductSearchResultModel
			{
				ProductId = item.Product.Id,
				ProductLogoId = PublicationUtils.GetLatestLogo(item.Product),
				ProductTitle = item.Product.Title,
				ProductType = item.Product.Type,
				AuthorTitle = item.Author.Title,
				Publications = LoadPublications(item.Product.Publications, cancellationToken).ToArray(),
				HasMorePublications = results.Count > SearchConstants.PublicationsPerProductLimit,
			};
		}
	}

	IEnumerable<ProductPublicationModel> LoadPublications(IEnumerable<AutoId> publicationsIds, CancellationToken cancellationToken)
	{
		foreach(var publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				yield break;

			Publication publication = mcv.Publications.Latest(publicationId);
			Store store = mcv.Stores.Latest(publication.Store);
			yield return new ProductPublicationModel
			{
				PublicationId = publication.Id.ToString(),
				StoreId = store.Id.ToString(),
				StoreTitle = store.Title,
				Rating = publication.Rating,
			};
		}
	}

	public TotalItemsResult<ProductPublicationModel> GetProductPublications(string productId, int page, int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(productId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {ProductId}, {Page}, {PageSize}", nameof(ProductsService), nameof(ProductsService.GetProductPublications), productId, page, pageSize);
#endif

		AutoId id = AutoId.Parse(productId);

		Product product = mcv.Products.Latest(id);
		if(product == null)
		{
			throw new EntityNotFoundException(nameof(Product).ToLower(), productId);
		}

		IEnumerable<AutoId> publicationsIds = product.Publications.Skip(page * pageSize).Take(pageSize);
		IEnumerable<ProductPublicationModel> publications = LoadPublications(publicationsIds, cancellationToken);

		return new TotalItemsResult<ProductPublicationModel> { Items = publications, TotalItems = product.Publications.Length };
	}

	//public IEnumerable<ProductSearchResultBaseModel> SearchLite([NotNull][NotEmpty] string query, [NonNegativeValue][NonZeroValue] int limit, CancellationToken cancellationToken)
	//{
	//	logger.LogDebug("{ClassName}.{MethodName} method called with {Query}, {Limit}", nameof(ProductsService), nameof(ProductsService.SearchLite), query, limit);

	//	ArgumentException.ThrowIfNullOrEmpty(query);
	//	ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

	//	var result = mcv.ProductTitles.Search(query, ProductType.Software, 0, limit);
	//	return MapTo<ProductSearchResultBaseModel>(result, cancellationToken);
	//}
}