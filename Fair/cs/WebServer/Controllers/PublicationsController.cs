using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	IPublicationsService publicationsService,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		entityIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		PublicationModel publication = publicationsService.GetPublication(publicationId);
		if (publication == null)
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		return publication;
	}

	[HttpGet("~/api/sites/{siteId}/publications")]
	public IEnumerable<SitePublicationModel> Search(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? title, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Search)} method called with {{SiteId}}, {{Pagination}}, {{Title}}", siteId, pagination, title);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		// TODO: validate search string: title
		paginationValidator.Validate(pagination);

		int page = pagination?.Page ?? 0;
		int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
		TotalItemsResult<SitePublicationModel> products = publicationsService.SearchPublicationsNonOptimized(siteId, page, pageSize, title, cancellationToken);
		if (products == null)
		{
			throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
		}

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
