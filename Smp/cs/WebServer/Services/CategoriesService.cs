using Ardalis.GuardClauses;

namespace Uccs.Smp;

public class CategoriesService
(
	ILogger<CategoriesService> logger,
	SmpMcv mcv
) : ICategoriesService
{
	public CategoryModel Find(string categoryId)
	{
		logger.LogDebug($"GET {nameof(CategoriesService)}.{nameof(CategoriesService.Find)} method called with {{CategoryId}}", categoryId);

		Guard.Against.NullOrEmpty(categoryId);

		EntityId id = EntityId.Parse(categoryId);

		Category category = null;
		Category parentCategory = null;
		lock (mcv.Lock)
		{
			category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			if (category == null)
			{
				return null;
			}

			if (category.Parent != null)
			{
				parentCategory = mcv.Categories.Find(category.Parent, mcv.LastConfirmedRound.Id);
			}
		}

		CategorySubModel[] categories = category.Categories.Length > 0 ? LoadCategories(category.Categories) : null;
		CategoryPublicationModel[] publications = category.Publications.Length > 0 ? LoadPublications(category.Publications) : null;

		return ToCategoryModel(category, parentCategory, categories, publications);
	}

	private CategorySubModel[] LoadCategories(EntityId[] categoriesIds)
	{
		lock (mcv.Lock)

		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			return new CategorySubModel(category);
		}).ToArray();
	}

	private CategoryPublicationModel[] LoadPublications(EntityId[] publicationsIds)
	{
		lock (mcv.Lock)

		return publicationsIds.Select(id =>
		{
			Publication publication = mcv.Publications.Find(id, mcv.LastConfirmedRound.Id);
			return new CategoryPublicationModel
			{
				Id = publication.Id.ToString(),
				Title = "PUBLICATION TITLE!!!!" + publication.Id.ToString(),
			};
		}).ToArray();
	}

	private static CategoryModel ToCategoryModel(Category category, Category parentCategory, CategorySubModel[] categories, CategoryPublicationModel[] publications)
	{
		return new CategoryModel
		{
			Id = category.Id.ToString(),
			Title = category.Title,
			SiteId = category.Site.ToString(),
			ParentId = category.Parent?.ToString(),
			ParentTitle = parentCategory?.Title,
			Categories = categories,
			Publications = publications
		};
	}
}
