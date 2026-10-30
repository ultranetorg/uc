using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/publishers")]
public class PublishersController
(
#if DEBUG
	ILogger<PublishersController> logger,
#endif
	PublishersService publishersService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PublisherModel> GetAll(string storeId, [FromQuery] string search, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Search}, {Pagination}", nameof(PublishersController), nameof(PublishersController.GetAll), storeId, search, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<PublisherModel> publishers = publishersService.GetPublishers(storeId, page, pageSize, search, cancellationToken);

		return this.OkPaged(publishers.Items, page, pageSize, publishers.TotalItems);
	}
}
