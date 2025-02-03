using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Uccs.Web.Utilities;

namespace Uccs.Smp;

public class AuthorsController
(
	ILogger<AuthorsController> logger,
	IEntityIdValidator entityIdValidator,
	IAuthorsService authorsService
): BaseController
{
	[HttpGet("{authorId}")]
	public AuthorModel Get(string authorId)
	{
		logger.LogInformation($"GET {nameof(AuthorsController)}.{nameof(AuthorsController.Get)} method called with {{AuthorId}}", authorId);

		entityIdValidator.Validate(authorId, nameof(Author).ToLower());

		AuthorModel author = authorsService.Find(authorId);
		if (author == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return author;
	}
}
