using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/surveys/moderators")]
public class ModeratorSurveysController
(
#if DEBUG
	ILogger<ModeratorSurveysController> logger,
#endif
	ModeratorSurveysService moderatorSurveysService
) : BaseController
{
	[HttpGet]
	public IEnumerable<SurveyModel> GetAll(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(ModeratorSurveysController), nameof(ModeratorSurveysController.GetAll), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<SurveyModel> result = moderatorSurveysService.GetAll(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}