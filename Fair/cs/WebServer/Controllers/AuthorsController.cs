using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AuthorsController
(
	ILogger<CategoriesController> logger,
	IEntityIdValidator entityIdValidator,
	IAuthorsService authorsService
) : BaseController
{
	[HttpGet("{authorId}")]
	public SiteAuthorModel Get(string authorId)
	{
		logger.LogInformation($"GET {nameof(AuthorsController)}.{nameof(AuthorsController.Get)} method called with {{SiteId}}, {{AuthorId}}", authorId);

		entityIdValidator.Validate(authorId, nameof(Author).ToLower());

		return authorsService.GetAuthor(authorId);
	}


	//[HttpGet("{siteId}/authors")]
	//public IEnumerable<AuthorBaseModel> GetAuthors(string siteId, [FromQuery] PaginationRequest pagination)
	//{
	//	logger.LogInformation($"GET {nameof(SitesController)}.{nameof(SitesController.GetAuthors)} method called with {{SiteId}}, {{Pagination}}", siteId, pagination);

	//	entityIdValidator.Validate(siteId, nameof(Site).ToLower());
	//	paginationValidator.Validate(pagination);

	//	int page = pagination?.Page ?? 0;
	//	int pageSize = pagination?.PageSize ?? Pagination.DefaultPageSize;
	//	TotalItemsResult<AuthorBaseModel> authors = sitesService.GetAuthors(siteId, page, pageSize);

	//	return this.OkPaged(authors.Items, page, pageSize, authors.TotalItems);
	//}
}
