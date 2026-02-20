using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/authors/{authorId}/files")]
public class AuthorFilesController
(
	ILogger<AuthorFilesController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> Get(string siteId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {AuthorId}", nameof(AuthorFilesController), nameof(Get), siteId, authorId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetAuthorFiles(siteId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
