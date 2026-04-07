using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/publications/unpublished")]
public class UnpublishedPublicationsController
(
	ILogger<UnpublishedPublicationsController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	UnpublishedPublicationsService unpublishedPublicationsSerivce
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationDetailsModel GetDetails(string siteId, string publicationId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {PublicationId}", nameof(UnpublishedPublicationsController), nameof(GetDetails), siteId, publicationId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		return unpublishedPublicationsSerivce.GetDetails(siteId, publicationId);
	}

	[HttpGet]
	public IEnumerable<UnpublishedProductModel> GetAll(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {Pagination}", nameof(UnpublishedPublicationsController), nameof(GetAll), siteId, pagination);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int pageValue, int pageSizeValue) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<UnpublishedProductModel> products = unpublishedPublicationsSerivce.GetAll(siteId, pageValue, pageSizeValue, cancellationToken);

		return this.OkPaged(products.Items, pageValue, pageSizeValue, products.TotalItems);
	}
}