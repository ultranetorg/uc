using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using NativeImport;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsService
(
	ILogger<PublicationsService> logger,
	FairMcv mcv
)
{
	public PublicationDetailsModel GetDetails([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetDetails), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		lock (mcv.Lock)
		{
			AutoId id = AutoId.Parse(publicationId);
			Publication publication = mcv.Publications.Latest(id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);

			// For unpublished publication Category is null.
			bool isPublicationPublished = publication.Category != null;
			Category? category = isPublicationPublished ? mcv.Categories.Latest(publication.Category) : null;

			var mappedFields = ProductFieldsUtils.GetMappedFields(product, publication);

			return new PublicationDetailsModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetTitle(publication, product),
				LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
				Updated = product.Updated.Hours,
				Fields = mappedFields,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString(),
				CategoryId = category?.Id.ToString(),
				CategoryTitle = category?.Title,
				Rating = isPublicationPublished ? publication.Rating : null
			};
		}
	}

	public PublicationVersionInfo GetVersions([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetVersions), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		lock(mcv.Lock)
		{
			AutoId id = AutoId.Parse(publicationId);
			Publication publication = mcv.Publications.Latest(id);
			if(publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			return new PublicationVersionInfo
			{
				Version = publication.ProductVersion,
				LatestVersion = product.Versions.Length - 1
			};
		}
	}

	public TotalItemsResult<PublicationAuthorModel> GetAuthorPublicationsNotOptimized([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {AuthorId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetAuthorPublicationsNotOptimized), siteId, authorId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock (mcv.Lock)
		{
			AutoId siteAutoId = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(siteAutoId);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId authorAutoId = AutoId.Parse(authorId);
			Author author = mcv.Authors.Latest(authorAutoId);
			if (author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			var context = new PublicationsContext
			{
				AuthorId = authorAutoId,
				Page = page,
				PageSize = pageSize,
				Items = new List<PublicationAuthorModel>(pageSize)
			};
			SearchInCategories(context, site.Categories, cancellationToken);

			return new TotalItemsResult<PublicationAuthorModel>
			{
				TotalItems = context.TotalItems,
				Items = context.Items,
			};
		}
	}

	void SearchInCategories(PublicationsContext context, IEnumerable<AutoId> categoriesIds, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (AutoId categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = null;

			lock (mcv.Lock)
			{
				category = mcv.Categories.Latest(categoryId);
			}

			SearchInPublications(context, category.Publications, cancellationToken);
			SearchInCategories(context, category.Categories, cancellationToken);
		}
	}

	void SearchInPublications(PublicationsContext context, AutoId[] publicationsIds, CancellationToken cancellationToken)
	{
		foreach (AutoId publicationId in publicationsIds)
		{
			Publication publication = null;
			Product product = null;

			lock (mcv.Lock)
			{
				publication = mcv.Publications.Latest(publicationId);
				product = mcv.Products.Latest(publication.Product);
			}

			if (product.Author != context.AuthorId)
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				AutoId? fileId = PublicationUtils.GetLogo(publication, product);
				byte[]? logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;
				var resultItem = new PublicationAuthorModel(publication, product, logo);
				context.Items.Add(resultItem);
			}

			++context.TotalItems;
		}
	}

	public TotalItemsResult<PublicationModel> GetCategoryPublicationsNotOptimized([NotNull][NotEmpty] string categoryId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {CategoryId}", nameof(PublicationsService), nameof(GetCategoryPublicationsNotOptimized), categoryId);

		Guard.Against.NullOrEmpty(categoryId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock (mcv.Lock)
		{
			AutoId id = AutoId.Parse(categoryId);
			Category category = mcv.Categories.Latest(id);
			if (category == null)
			{
				throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
			}

			if (category.Publications.Length == 0)
			{
				return TotalItemsResult<PublicationModel>.Empty;
			}

			var context = new SearchContext<PublicationModel>
			{
				Page = page,
				PageSize = pageSize,
				Items = new List<PublicationModel>(pageSize)
			};
			LoadPublications(category, context, cancellationToken);

			return new TotalItemsResult<PublicationModel>
			{
				Items = context.Items,
				TotalItems = context.TotalItems
			};
		}
	}

	void LoadPublications(Category category, SearchContext<PublicationModel> context, CancellationToken cancellationToken)
	{
		foreach(AutoId publicationId in category.Publications)
		{
			if(cancellationToken.IsCancellationRequested)
				return;

			if(context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				Publication publication = mcv.Publications.Latest(publicationId);
				Product product = mcv.Products.Latest(publication.Product);
				AutoId? fileId = PublicationUtils.GetLogo(publication, product);
				byte[] logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;

				var model = new PublicationModel(publication, product, category, logo);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}

	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublicationsNotOptimized([NotNull][NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(PublicationsService), nameof(GetCategoriesPublicationsNotOptimized), siteId);

		Guard.Against.NullOrEmpty(siteId);

		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<CategoryPublicationsModel>();

		lock (mcv.Lock)
		{
			AutoId id = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(id);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site), siteId);
			}
			if (site.Categories.Length == 0)
			{
				return Enumerable.Empty<CategoryPublicationsModel>();
			}

			var result = new List<CategoryPublicationsModel>(site.Categories.Length);
			LoadCategoriesPublications(site.Categories, result, cancellationToken);

			return result;
		}
	}

	void LoadCategoriesPublications(AutoId[] categoriesIds, IList<CategoryPublicationsModel> result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach (AutoId categoryId in categoriesIds)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Latest(categoryId);

			var resultCategory = new CategoryPublicationsModel(category)
			{
				Publications = new List<PublicationExtendedModel>(CategoriesPublications.DefaultPublicationsCount)
			};

			LoadPublicationsFromCategory(category, resultCategory, cancellationToken);

			if (cancellationToken.IsCancellationRequested)
				return;

			if (resultCategory.Publications.Count > 0)
			{
				result.Add(resultCategory);
			}
		}
	}

	void LoadPublicationsFromCategory(Category category, CategoryPublicationsModel result, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		if (result.Publications.Count >= CategoriesPublications.DefaultPublicationsCount)
		{
			return;
		}

		foreach (AutoId publicationId in category.Publications)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);
			byte[]? logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;

			var model = new PublicationExtendedModel(publication, product, author, category, logo);
			result.Publications.Add(model);

			if (result.Publications.Count >= CategoriesPublications.DefaultPublicationsCount)
			{
				return;
			}
		}

		foreach (AutoId subCategoryId in category.Categories)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			Category subCategory = mcv.Categories.Latest(subCategoryId);
			LoadPublicationsFromCategory(subCategory, result, cancellationToken);
		}
	}

	public ChangedPublicationDetailsModel GetChangedPublicationDetails([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string changedPublicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ChangedPublicationId}", nameof(PublicationsService), nameof(GetChangedPublicationDetails), siteId, changedPublicationId);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(changedPublicationId);

		lock(mcv.Lock)
		{
			AutoId entitySiteId = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(entitySiteId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId entityChangedPublicationId = AutoId.Parse(changedPublicationId);
			if(!site.ChangedPublications.Contains(entityChangedPublicationId))
			{
				throw new EntityNotFoundException(EntityNames.ChangedPublicationEntityName, changedPublicationId);
			}
			
			Publication publication = mcv.Publications.Latest(entityChangedPublicationId);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			var fields = ProductFieldsUtils.GetMappedFieldsVersion(product, publication.ProductVersion);
			var fieldsTo = ProductFieldsUtils.GetLatestMappedFields(product);

			return new ChangedPublicationDetailsModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetTitle(publication, product),
				LogoId = fileId?.ToString(),
				Updated = product.Updated.Hours,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString(),
				CurrentVersion = publication.ProductVersion,
				LatestVersion = product.Versions.Last().Id,
				CategoryId = category.Id.ToString(),
				CategoryTitle = category.Title,
				Rating = publication.Rating,
				Fields = fields,
				FieldsTo = fieldsTo
			};
		}
	}

	public TotalItemsResult<ChangedPublicationModel> GetChangedPublicationsAll([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetChangedPublicationsAll), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock(mcv.Lock)
		{
			AutoId id = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(site.ChangedPublications.Length == 0)
			{
				return TotalItemsResult<ChangedPublicationModel>.Empty;
			}

			IEnumerable<AutoId> publicationsIds = site.ChangedPublications.Skip(page * pageSize).Take(pageSize);
			List<ChangedPublicationModel> result = LoadChangedPublications<ChangedPublicationModel>(publicationsIds, pageSize, cancellationToken);

			return new TotalItemsResult<ChangedPublicationModel>
			{
				Items = result,
				TotalItems = site.ChangedPublications.Length
			};
		}
	}

	List<ChangedPublicationModel> LoadChangedPublications<T>(IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<ChangedPublicationModel> result = new(pageSize);
		foreach(AutoId publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			var model = new ChangedPublicationModel
			{
				Id = publication.Id.ToString(),
				Type = product.Type,
				Title = PublicationUtils.GetTitle(publication, product),
				LogoId = fileId?.ToString(),
				Updated = product.Updated.Hours,
				AuthorId = author.Id.ToString(),
				AuthorTitle = author.Title,
				AuthorLogoId = author.Avatar?.ToString(),
				CategoryId = category.Id.ToString(),
				CategoryTitle = category.Title,
				CurrentVersion = publication.ProductVersion,
				LatestVersion = product.Versions.Last().Id
			};
			result.Add(model);
		}

		return result;
	}

	TotalItemsResult<T> GetSitePublications<T>(string siteId, Func<Site, AutoId[]> GetPublications, Func<Product, Publication, T> CreatePublication, int page, int pageSize, CancellationToken cancellationToken)
		where T: PublicationBaseModel
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetSitePublications), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		lock(mcv.Lock)
		{
			AutoId id = AutoId.Parse(siteId);
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			AutoId[] publications = GetPublications(site);

			if(publications.Length == 0)
			{
				return TotalItemsResult<T>.Empty;
			}

			IEnumerable<AutoId> publicationsIds = publications.Skip(page * pageSize).Take(pageSize);
			List<T> result = LoadPublications(CreatePublication, publicationsIds, pageSize, cancellationToken);

			return new TotalItemsResult<T>
			{
				Items = result,
				TotalItems = publications.Length
			};
		}
	}

	List<T> LoadPublications<T>(Func<Product, Publication, T> CreatePublication, IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
		where T : PublicationBaseModel
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		var result = new List<T>(pageSize);
		foreach (AutoId publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			Publication publication = mcv.Publications.Latest(publicationId);
			Product product = mcv.Products.Latest(publication.Product);
			T model = CreatePublication(product, publication);
			result.Add(model);
		}

		return result;
	}

	class PublicationsContext : SearchContext<PublicationAuthorModel>
	{
		public AutoId AuthorId { get; set; }
	}
}
