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
	public PublicationDetailsModel GetDetails([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetDetails), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

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

		IEnumerable<CategoryPathItem>? path = category != null ? PublicationUtils.BuildPath(mcv, category).Reverse() : null;

		return new PublicationDetailsModel
		{
			Id = publication.Id.ToString(),
			StoreId = publication.Store.ToString(),
			Type = product.Type,
			Title = PublicationUtils.GetTitle(publication, product),
			LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
			Updated = product.Updated.Hours,
			Fields = mappedFields,
			AuthorId = author.Id.ToString(),
			AuthorTitle = author.Title,
			AuthorLogoId = author.Avatar?.ToString(),
			Path = path,
			Rating = isPublicationPublished ? publication.Rating : null
		};
	}

	public PublicationVersionInfo GetVersions([NotEmpty][NotNull] string publicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {PublicationId}", nameof(PublicationsService), nameof(GetVersions), publicationId);

		Guard.Against.NullOrEmpty(publicationId);

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

	public TotalItemsResult<PublicationAuthorModel> GetPublisherPublicationsNotOptimized([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {AuthorId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetPublisherPublicationsNotOptimized), storeId, authorId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId storeEntityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(storeEntityId);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
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
		SearchInCategories(context, store.Categories, cancellationToken);

		return new TotalItemsResult<PublicationAuthorModel>
		{
			TotalItems = context.TotalItems,
			Items = context.Items,
		};
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

			category = mcv.Categories.Latest(categoryId);

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

			publication = mcv.Publications.Latest(publicationId);
			product = mcv.Products.Latest(publication.Product);

			if (product.Author != context.AuthorId)
			{
				continue;
			}

			if (context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				Category category = mcv.Categories.Latest(publication.Category);

				AutoId? fileId = PublicationUtils.GetLogo(publication, product);
				byte[]? logo = fileId != null ? mcv.Files.Latest(fileId).Data : null;
				var resultItem = new PublicationAuthorModel(publication, product)
				{
					ProductId = product.Id.ToString(),
					LogoId = PublicationUtils.GetLogo(publication, product)?.ToString(),
					CategoryId = publication.Category.ToString(),
					CategoryTitle = category.Title
				};
				context.Items.Add(resultItem);
			}

			++context.TotalItems;
		}
	}

	public TotalItemsResult<PublicationExtendedModel> GetCategoryPublicationsNotOptimized([NotNull][NotEmpty] string categoryId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {CategoryId}", nameof(PublicationsService), nameof(GetCategoryPublicationsNotOptimized), categoryId);

		Guard.Against.NullOrEmpty(categoryId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(categoryId);
		Category category = mcv.Categories.Latest(id);
		if (category == null)
		{
			throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
		}

		if (category.Publications.Length == 0)
		{
			return TotalItemsResult<PublicationExtendedModel>.Empty;
		}

		var context = new SearchContext<PublicationExtendedModel>
		{
			Page = page,
			PageSize = pageSize,
			Items = new List<PublicationExtendedModel>(pageSize)
		};
		LoadPublications(category, context, cancellationToken);

		return new TotalItemsResult<PublicationExtendedModel>
		{
			Items = context.Items,
			TotalItems = category.Publications.Length
		};
	}

	void LoadPublications(Category category, SearchContext<PublicationExtendedModel> context, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
			return;

		foreach(AutoId publicationId in category.Publications)
		{
			if(cancellationToken.IsCancellationRequested)
				return;

			if(context.TotalItems >= context.Page * context.PageSize && context.TotalItems < (context.Page + 1) * context.PageSize)
			{
				Publication publication = mcv.Publications.Latest(publicationId);
				Product product = mcv.Products.Latest(publication.Product);
				AutoId? fileId = PublicationUtils.GetLogo(publication, product);
				Author author = mcv.Authors.Latest(product.Author);

				var model = new PublicationExtendedModel(publication, product, author, category);
				context.Items.Add(model);
			}

			++context.TotalItems;
		}
	}

	public IEnumerable<CategoryPublicationsModel> GetCategoriesPublicationsNotOptimized([NotNull][NotEmpty] string storeId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(PublicationsService), nameof(GetCategoriesPublicationsNotOptimized), storeId);

		Guard.Against.NullOrEmpty(storeId);

		if (cancellationToken.IsCancellationRequested)
			return Enumerable.Empty<CategoryPublicationsModel>();

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store), storeId);
		}
		if (store.Categories.Length == 0)
		{
			return Enumerable.Empty<CategoryPublicationsModel>();
		}

		var result = new List<CategoryPublicationsModel>(store.Categories.Length);
		LoadCategoriesPublications(store.Categories, result, cancellationToken);

		return result;
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

			var model = new PublicationExtendedModel(publication, product, author, category);
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

	public ChangedPublicationDetailsModel GetChangedPublicationDetails([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string changedPublicationId)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {ChangedPublicationId}", nameof(PublicationsService), nameof(GetChangedPublicationDetails), storeId, changedPublicationId);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.NullOrEmpty(changedPublicationId);

		AutoId storeEntityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		AutoId entityChangedPublicationId = AutoId.Parse(changedPublicationId);
		if(!store.ChangedPublications.Contains(entityChangedPublicationId))
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

	public TotalItemsResult<ChangedPublicationModel> GetChangedPublicationsAll([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetChangedPublicationsAll), storeId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if(store.ChangedPublications.Length == 0)
		{
			return TotalItemsResult<ChangedPublicationModel>.Empty;
		}

		IEnumerable<AutoId> publicationsIds = store.ChangedPublications.Skip(page * pageSize).Take(pageSize);
		IEnumerable<AutoId> reversed = publicationsIds.Reverse();
		List<ChangedPublicationModel> result = LoadChangedPublications<ChangedPublicationModel>(store, reversed, pageSize, cancellationToken);

		return new TotalItemsResult<ChangedPublicationModel>
		{
			Items = result,
			TotalItems = store.ChangedPublications.Length
		};
	}

	bool HasProductUpdationProposalForProduct(Store store, AutoId publicationId)
	{
		return store.Proposals.Any(x =>
		{
			Proposal proposal = mcv.Proposals.Latest(x);
			if(proposal.OptionClass != FairOperationClass.PublicationUpdation)
			{
				return false;
			}

			return (proposal.Options[0].Operation as PublicationUpdation).Publication == publicationId;
		});
	}

	List<ChangedPublicationModel> LoadChangedPublications<T>(Store store, IEnumerable<AutoId> publicationsIds, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<ChangedPublicationModel> result = new(pageSize);
		foreach(AutoId publicationId in publicationsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			if (HasProductUpdationProposalForProduct(store, publicationId))
				continue;

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

	TotalItemsResult<T> GetStorePublications<T>(string storeId, Func<Store, AutoId[]> GetPublications, Func<Product, Publication, T> CreatePublication, int page, int pageSize, CancellationToken cancellationToken)
		where T: PublicationBaseModel
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(PublicationsService), nameof(GetStorePublications), storeId, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		AutoId[] publications = GetPublications(store);

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
