using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublishersController(ILogger<PublishersController> logger, AutoIdValidator autoIdValidator, PaginationValidator paginationValidator, PublicationsService publicationsService) : BaseController
{
	[HttpGet("~/api/sites/{siteId}/publishers/{publisherId}/publications")]
	public IEnumerable<PublicationAuthorModel> GetPublications(string siteId, string publisherId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {PublisherId}, {Pagination}", nameof(PublishersController), nameof(PublishersController.GetPublications), siteId, publisherId, pagination);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		autoIdValidator.Validate(publisherId, nameof(Publisher).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublicationAuthorModel> products = publicationsService.GetPublisherPublicationsNotOptimized(siteId, publisherId, page, pageSize, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
