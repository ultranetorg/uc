using Ardalis.GuardClauses;

namespace Uccs.Smp;

public class PublicationsService
(
	ILogger<PublicationsService> logger,
	SmpMcv mcv
) : IPublicationsService
{
	public PublicationModel Find(string publicationId)
	{
		logger.LogDebug($"GET {nameof(PublicationsService)}.{nameof(PublicationsService.Find)} method called with {{PublicationId}}", publicationId);

		Guard.Against.NullOrEmpty(publicationId);

		EntityId entityId = EntityId.Parse(publicationId);

		Publication publication = null;
		Product product = null;
		Author author = null;
		lock (mcv.Lock)
		{
			publication = mcv.Publications.Find(entityId, mcv.LastConfirmedRound.Id);
			if (publication == null)
			{
				return null;
			}

			product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			author = mcv.Authors.Find(product.AuthorId, mcv.LastConfirmedRound.Id);
		}

		return ToPublicationModel(publication, product, author);
	}

	private static PublicationModel ToPublicationModel(Publication publication, Product product, Author author)
	{
		return new PublicationModel
		{
			Id = publication.Id.ToString(),
			CategoryId = publication.Category.ToString(),
			CreatorId = publication.Creator.ToString(),
			ProductId = product.Id.ToString(),
			ProductName = ProductUtils.GetTitle(product),
			ProductFields = product.Fields,
			ProductUpdated = product.Updated.Days,
			ProductAuthorId = author.Id.ToString(),
			ProductAuthorTitle = author.Title,
			Sections = publication.Sections,
			// TODO: map comments Comments
		};
	}
}
