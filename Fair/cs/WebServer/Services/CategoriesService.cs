using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class CategoriesService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
) : ICategoriesService
{
	public CategoryModel GetCategory(string categoryId, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(CategoriesService)}.{nameof(CategoriesService.GetCategory)} method called with {{CategoryId}}", categoryId);

		Guard.Against.NullOrEmpty(categoryId);

		AutoId id = AutoId.Parse(categoryId);

		lock (mcv.Lock)
		{
			Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			if (category == null)
			{
				throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
			}

			Category parentCategory = null;
			if (category.Parent != null)
			{
				parentCategory = mcv.Categories.Find(category.Parent, mcv.LastConfirmedRound.Id);
			}

			IEnumerable<CategoryBaseModel> categories = category.Categories.Length > 0 ? LoadCategories(category.Categories) : [];

			return new CategoryModel(category, parentCategory?.Title.ToString())
			{
				Categories = categories,
			};
		}
	}

	IEnumerable<CategoryBaseModel> LoadCategories(AutoId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Find(id, mcv.LastConfirmedRound.Id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}

	public IEnumerable<CategoryParentBaseModel> GetCategories(string siteId, int depth, CancellationToken cancellationToken)
	{
		logger.LogDebug($"GET {nameof(CategoriesService)}.{nameof(CategoriesService.GetCategories)} method called with {{SiteId}}, {{Depth}}", siteId, depth);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NegativeOrZero(depth);

		AutoId id = AutoId.Parse(siteId);

		lock (mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			if (site.Categories.Length == 0)
			{
				return [];
			}

			List<CategoryParentBaseModel> result = new List<CategoryParentBaseModel>(site.Categories.Length);
			LoadCategoriesRecursively(site.Categories, depth, ref result, cancellationToken);

			return result;
		}
	}

	void LoadCategoriesRecursively(AutoId[] categories, int currentDepth, ref List<CategoryParentBaseModel> result, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return;

		foreach(AutoId id in categories)
		{
			if(cancellationToken.IsCancellationRequested)
				return;

			Category category = mcv.Categories.Latest(id);

			CategoryParentBaseModel model = new CategoryParentBaseModel(category);
			result.Add(model);

			if (category.Categories.Length > 0 && currentDepth > 1)
			{
				result.Capacity += category.Categories.Length;
				LoadCategoriesRecursively(category.Categories, currentDepth - 1, ref result, cancellationToken);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IsApprovedStatus(Publication publication) => publication.Status == PublicationStatus.Approved;
}
