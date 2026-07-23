using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/stores/{storeId}/authors/{authorId}/files")]
public class AuthorFilesController
(
	ILogger<AuthorFilesController> logger,
	AutoIdValidator autoIdValidator,
	PaginationValidator paginationValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public IEnumerable<FileModel> GetAll(string storeId, string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {AuthorId}, {Pagination}", nameof(AuthorFilesController), nameof(GetAll), storeId, authorId, pagination);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		autoIdValidator.Validate(authorId, nameof(Author).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<FileModel> referendums = filesService.GetAuthorFiles(storeId, authorId, page, pageSize, cancellationToken);

		return this.OkPaged(referendums.Items, page, pageSize, referendums.TotalItems);
	}
}
