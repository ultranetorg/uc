using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class AuthorsController
(
	ILogger<AuthorsController> logger,
	AuthorsService authorsService,
	SearchService searchService,
	AutoIdValidator autoIdValidator,
	SearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator,
	PaginationValidator paginationValidator
) : BaseController
{
	[HttpGet("{authorId}")]
	public AuthorDetailsModel GetDetails(string authorId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {AuthorId}", nameof(AuthorsController), nameof(GetDetails), authorId);

		autoIdValidator.Validate(authorId, nameof(Author).ToLower());

		return authorsService.GetDetails(authorId);
	}

	[HttpGet]
	public IEnumerable<AuthorBaseAvatarModel> SearchAuthors([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {Query}, {Limit}", nameof(AuthorsController), nameof(SearchAuthors), query, limit);

		searchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchAuthors(query, limit ?? SearchConstants.SearchUsersLimit, cancellationToken);
	}

	[HttpGet("{authorId}/products")]
	public IEnumerable<ProductAuthorModel> GetProducts(string authorId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {AuthorId}, {Pagination}", nameof(AuthorsController), nameof(AuthorsController.GetProducts), authorId, pagination);

		autoIdValidator.Validate(authorId, nameof(Store).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProductAuthorModel> products = authorsService.GetProducts(authorId, page, pageSize, cancellationToken);

		return this.OkPaged(products.Items, page, pageSize, products.TotalItems);
	}
}
