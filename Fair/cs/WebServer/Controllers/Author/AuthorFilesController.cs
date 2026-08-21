using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/authors/{authorId}/files")]
public class AuthorFilesController
(
#if DEBUG
	ILogger<AuthorFilesController> logger,
#endif
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> GetAll(string storeId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {AuthorId}, {Pagination}", nameof(AuthorFilesController), nameof(AuthorFilesController.GetAll), storeId, authorId, pagination);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(authorId, nameof(Author).ToLower());
		PaginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetAuthorFiles(storeId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
