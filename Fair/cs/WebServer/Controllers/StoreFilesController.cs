using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/files")]
public class StoreFilesController
(
#if DEBUG
	ILogger<StoreFilesController> logger,
#endif
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> Get(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Pagination}", nameof(StoreFilesController), nameof(StoreFilesController.Get), storeId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetStoreFiles(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
