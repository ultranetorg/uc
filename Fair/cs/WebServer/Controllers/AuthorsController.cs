using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class AuthorsController
(
	ILogger<AuthorsController> logger,
	IAutoIdValidator autoIdValidator,
	IAuthorsService authorsService
) : BaseController
{
	[HttpGet("{authorId}")]
	public AuthorDetailsModel Get(string authorId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {AuthorId}", nameof(AuthorsController), nameof(Get), authorId);

		autoIdValidator.Validate(authorId, nameof(Author).ToLower());

		return authorsService.GetDetails(authorId);
	}
}
