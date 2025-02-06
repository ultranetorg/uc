using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	IPublicationsService publicationsService,
	IEntityIdValidator entityIdValidator
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		entityIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		PublicationModel publication = publicationsService.Find(publicationId);
		if (publication == null)
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		return publication;
	}
}
