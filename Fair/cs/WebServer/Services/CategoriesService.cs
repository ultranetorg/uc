using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class CategoriesService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
) : ICategoriesService
{
	public CategoryModel GetCategory(string categoryId)
	{
		logger.LogDebug($"GET {nameof(CategoriesService)}.{nameof(CategoriesService.GetCategory)} method called with {{CategoryId}}", categoryId);

		Guard.Against.NullOrEmpty(categoryId);

		EntityId categoryEntityId = EntityId.Parse(categoryId);

		lock (mcv.Lock)
		{
			Category category = mcv.Categories.Find(categoryEntityId, mcv.LastConfirmedRound.Id);
			if (category == null)
			{
				throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
			}

			Category parentCategory = null;
			if (category.Parent != null)
			{
				parentCategory = mcv.Categories.Find(category.Parent, mcv.LastConfirmedRound.Id);
			}

			IEnumerable<CategoryBaseModel> categories = category.Categories.Length > 0 ? LoadCategories(category.Categories) : null;
			IEnumerable<PublicationBaseModel> publications = category.Publications.Length > 0 ? LoadPublications(category.Publications) : null;

			return new CategoryModel(category, parentCategory?.Title.ToString())
			{
				Categories = categories,
				Publications = publications,
			};
		}
	}

	private IEnumerable<CategoryBaseModel> LoadCategories(EntityId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}

	private IEnumerable<PublicationBaseModel> LoadPublications(EntityId[] publicationsIds)
	{
		return publicationsIds.Select(id =>
		{
			Publication publication = mcv.Publications.Find(id, mcv.LastConfirmedRound.Id);
			Product product = mcv.Products.Find(publication.Product, mcv.LastConfirmedRound.Id);
			return new PublicationBaseModel(publication, product);
		}).ToArray();
	}
}
