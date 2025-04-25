using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	IPublicationsService publicationsService
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		entityIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetPublication(publicationId);
	}

	[HttpGet("~/api/sites/{siteId}/publications")]
	public IEnumerable<PublicationSearchModel> Search(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? title, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(PublicationsController.Search)} method called with {{SiteId}}, {{Pagination}}, {{Title}}", siteId, pagination, title);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: title
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationSearchModel> products = publicationsService.SearchPublicationsNotOptimized(siteId, page, pageSize, title, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}

	[HttpGet("~/api/sites/{siteId}/authors/{authorId}/publications")]
	public IEnumerable<PublicationAuthorModel> GetAuthorPublications(string siteId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{SiteId}}, {{AuthorId}}, {{Pagination}}", siteId, authorId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationAuthorModel> products = publicationsService.GetAuthorPublicationsNotOptimized(siteId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}

	[HttpGet("~/api/categories/{categoryId}/publications")]
	public IEnumerable<PublicationModel> GetCategoryPublications(string categoryId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.GetCategoryPublications)} method called with {{CategoryId}}, {{Pagination}}", categoryId, pagination);

		entityIdValidator.Validate(categoryId, nameof(Category).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationModel> publications = publicationsService.GetCategoryPublicationsNotOptimized(categoryId, page, pageSize, cancellationToken);

		return this.OkPaged(publications.Items, page, pageSize, publications.TotalItems);
	}
}
