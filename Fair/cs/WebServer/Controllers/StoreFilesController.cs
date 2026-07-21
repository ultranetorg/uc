using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/files")]
public class StoreFilesController
(
	ILogger<StoreFilesController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> Get(string storeId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(StoreFilesController), nameof(Get), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetStoreFiles(storeId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
