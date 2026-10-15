using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/surveys/recall-moderators")]
public class ModeratorRecallSurveysController
(
#if DEBUG
	ILogger<ModeratorRecallSurveysController> logger,
#endif
	ModeratorRecallSurveysService moderatorRecallSurveysService
) : BaseController
{
	[HttpGet]
	public IEnumerable<SurveyModel> Get(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(ModeratorRecallSurveysController), nameof(ModeratorRecallSurveysController.Get), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<SurveyModel> result = moderatorRecallSurveysService.GetSurveys(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(result.Items, page, pageSize, result.TotalItems);
	}
}