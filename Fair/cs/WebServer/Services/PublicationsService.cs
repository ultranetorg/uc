using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class PublicationsService
(
	ILogger<PublicationsService> logger,
	FairMcv mcv
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
			author = mcv.Authors.Find(product.Author, mcv.LastConfirmedRound.Id);
		}

		var reviews = publication.Reviews.Length > 0 ? LoadReviews(publication.Reviews) : null;

		return new PublicationModel(publication, product, author)
		{
			ProductFields = GetProductFields(publication, product),
			Reviews = reviews
		};
	}

	private IEnumerable<ReviewModel> LoadReviews(EntityId[] reviewsIds)
	{
		lock (mcv.Lock)
		{
			return reviewsIds.Select(id =>
			{
				Review review = mcv.Reviews.Find(id, mcv.LastConfirmedRound.Id);
				Account account = mcv.Accounts.Find(review.Creator, mcv.LastConfirmedRound.Id);
				return new ReviewModel(review, account);
			}).ToArray(); ;
		}
	}

	private IEnumerable<ProductFieldModel> GetProductFields(Publication publication, Product product)
	{
		return publication.Fields.Select(x => new ProductFieldModel
		{
			Name = x.Name,
			Value = product.Fields.FirstOrDefault(field => field.Name == x.Name)?
						   .Versions.FirstOrDefault(version => version.Version == x.Version)?.Value,
		});
	}
}
