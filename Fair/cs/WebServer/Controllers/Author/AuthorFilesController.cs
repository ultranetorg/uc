using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/authors/{authorId}/files")]
public class AuthorFilesController
(
	ILogger<AuthorFilesController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> GetAll(string siteId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {AuthorId}, {Pagination}", nameof(AuthorFilesController), nameof(GetAll), siteId, authorId, pagination);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		autoIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetAuthorFiles(siteId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
