using System.Threading;
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
	public PublicationModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		entityIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return publicationsService.GetPublication(publicationId);
	}

	[HttpGet("~/sites/{siteId}/publications")]
	public IEnumerable<SitePublicationModel> Search(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? title, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(SitesController)}.{nameof(PublicationsController.Search)} method called with {{SiteId}}, {{Pagination}}, {{Title}}", siteId, pagination, title);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: title
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<SitePublicationModel> products = publicationsService.SearchPublicationsNonOptimized(siteId, page, pageSize, title, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}

	[HttpGet("~/sites/{siteId}/authors/{authorId}/publications")]
	public IEnumerable<PublicationBaseModel> GetAuthorPublications(string siteId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{SiteId}}, {{AuthorId}}, {{Pagination}}", siteId, authorId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		entityIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationBaseModel> products = publicationsService.GetAuthorPublicationsNotOptimized(siteId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
