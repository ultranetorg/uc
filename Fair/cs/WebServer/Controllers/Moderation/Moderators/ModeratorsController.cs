using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair.Moderation;

[Route("api/moderation/stores/{storeId}/moderators")]
public class ModeratorsController
(
#if DEBUG
	ILogger<ModeratorsController> logger,
#endif
	ModeratorsService moderatorsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<ModeratorModel> GetAll(string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(ModeratorsController), nameof(ModeratorsController.GetAll), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return moderatorsService.GetModerators(storeId, cancellationToken);
	}
}
