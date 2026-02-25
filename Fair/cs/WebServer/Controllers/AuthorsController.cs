using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AuthorsController
(
	ILogger<AuthorsController> logger,
	IAuthorsService authorsService,
	SearchService searchService,
	IAutoIdValidator autoIdValidator,
	ISearchQueryValidator searchQueryValidator,
	LimitValidator limitValidator
) : BaseController
{
	[HttpGet("{authorId}")]
	public AuthorDetailsModel Get(string authorId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AuthorId}", nameof(AuthorsController), nameof(Get), authorId);

		autoIdValidator.Validate(authorId, nameof(Author).ToLower());

		return authorsService.GetDetails(authorId);
	}

	[HttpGet]
	public IEnumerable<AuthorBaseAvatarModel> SearchAuthors([FromQuery] string? query, [FromQuery] int? limit, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {Query}, {Limit}", nameof(AuthorsController), nameof(SearchAuthors), query, limit);

		searchQueryValidator.Validate(query);
		limitValidator.Validate(limit);

		return searchService.SearchAuthors(query, limit ?? SearchConstants.SearchAccountsLimit, cancellationToken);
	}
}
