using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/files")]
public class SiteFilesController
(
	ILogger<SiteFilesController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> Get(string siteId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}", nameof(SiteFilesController), nameof(Get), siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetSiteFiles(siteId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
