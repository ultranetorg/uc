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
		logger.LogInformation($"GET {nameof(AuthorsController)}.{nameof(AuthorsController.Get)} method called with {{AuthorId}}", authorId);

		autoIdValidator.Validate(authorId, nameof(Author).ToLower());

		return authorsService.GetDetails(authorId);
	}
}
