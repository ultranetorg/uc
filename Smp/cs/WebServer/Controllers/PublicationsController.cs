using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Utilities;

namespace Uccs.Smp;

public class PublicationsController
(
	ILogger<PublicationsController> logger,
	IPublicationsService productsService,
	IEntityIdValidator entityIdValidator
) : BaseController
{
	[HttpGet("{publicationId}")]
	public PublicationModel Get(string publicationId)
	{
		logger.LogInformation($"GET {nameof(PublicationsController)}.{nameof(PublicationsController.Get)} method called with {{PublicationId}}", publicationId);

		entityIdValidator.Validate(publicationId, nameof(Publication).ToLower());

		PublicationModel product = productsService.Find(publicationId);
		if (product == null)
		{
			throw new EntityNotFoundException(nameof(Publication).ToLower(), publicationId);
		}

		return product;
	}
}
