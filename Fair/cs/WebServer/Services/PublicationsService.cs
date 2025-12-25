using System.Diagnostics.CodeAnalysis;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsService
(
	ILogger<PublicationsService> logger,
	FairMcv mcv
)
{
	public PublicationDetailsModel GetPublicationDetails([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetPublicationDetails), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		AutoId id = AutoId.Parse(publicationId);

		lock (mcv.Lock)
		{
			Publication publication = mcv.Publications.Latest(id);
			if (publication == null)
			{
				throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
			}

			Product product = mcv.Products.Latest(publication.Product);
			Author author = mcv.Authors.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);
			byte[]? logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;
			byte[]? authorAvatar = author.Avatar != null ? mcv.Files.Latest(author.Avatar).Data : null;

			var fields = product.Versions.FirstOrDefault(i => i.Id == publication.ProductVersion)?.Fields;
			var mappedFields = fields != null ? ProductsService.MapValues(fields, Product.FindDeclaration(product.Type)) : [];

			return new PublicationDetailsModel(publication, product, author, category, logo, authorAvatar)
			{
				ProductFields = mappedFields
			};
		}
	}

	public PublicationVersionInfo GetVersions([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetVersions), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		AutoId id = AutoId.Parse(publicationId);

		lock(mcv.Lock)
		{
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

		AutoId siteAutoId = AutoId.Parse(siteId);
		AutoId authorAutoId = AutoId.Parse(authorId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteAutoId);
			if (site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

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

		AutoId id = AutoId.Parse(categoryId);

		lock (mcv.Lock)
		{
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

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
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

	public ChangedPublicationDetailsModel GetChangedPublication([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string changedPublicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {ChangedPublicationId}", nameof(PublicationsService), nameof(GetChangedPublication), siteId, changedPublicationId);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(changedPublicationId);

		AutoId entitySiteId = AutoId.Parse(siteId);
		AutoId entityChangedPublicationId = AutoId.Parse(changedPublicationId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(entitySiteId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(!site.ChangedPublications.Contains(entityChangedPublicationId))
			{
				throw new EntityNotFoundException(nameof(EntityNames.ChangedPublicationEntityName).ToLower(), changedPublicationId);
			}

			Publication publication = mcv.Publications.Latest(entityChangedPublicationId);
			Product product = mcv.Products.Latest(publication.Product);
			if(product.Versions.Length < 2)
			{
				throw new InvalidPublicationVersionException(changedPublicationId);
			}

			FairAccount account = (FairAccount)mcv.Accounts.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			var fieldsFrom = product.Versions.Single(x => x.Id == publication.ProductVersion).Fields;
			var fieldsTo = product.Versions.OrderBy(x => x.Id).LastOrDefault()?.Fields;
			var mappedFrom = ProductsService.MapValues(fieldsFrom, Product.Software);
			var mappedTo = ProductsService.MapValues(fieldsTo, Product.Software);

			return new ChangedPublicationDetailsModel(publication.Id.ToString(), product, publication.ProductVersion, account, category, fileId)
			{
				From = mappedFrom,
				To = mappedTo
			};
		}
	}

	public TotalItemsResult<ChangedPublicationModel> GetChangedPublications([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetChangedPublications), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
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
				TotalItems = site.UnpublishedPublications.Length
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
			FairAccount account = (FairAccount)mcv.Accounts.Latest(product.Author);
			Category category = mcv.Categories.Latest(publication.Category);
			AutoId? fileId = PublicationUtils.GetLogo(publication, product);

			var model = new ChangedPublicationModel(publication.Id.ToString(), product, publication.ProductVersion, account, category, fileId);
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

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
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

	private class PublicationsContext : SearchContext<PublicationAuthorModel>
	{
		public AutoId AuthorId { get; set; }
	}
}
