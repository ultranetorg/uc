using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class CategoriesService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
)
{
	public IEnumerable<CategoryBaseModel> GetRoot([NotNull][NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(CategoriesService), nameof(GetRoot), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId id = AutoId.Parse(siteId);

		Store site = mcv.Stores.Latest(id);
		if (site == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), siteId);
		}
		if (site.Categories.Length == 0)
		{
			return [];
		}

		return LoadCategories(site.Categories);
	}

	public CategoryModel GetDetails([NotNull][NotEmpty] string categoryId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {CategoryId}", nameof(CategoriesService), nameof(GetDetails), categoryId);

		Guard.Against.NullOrEmpty(categoryId);

		AutoId id = AutoId.Parse(categoryId);

		Category category = mcv.Categories.Latest(id);
		if (category == null)
		{
			throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
		}

		Category parentCategory = null;
		if (category.Parent != null)
		{
			parentCategory = mcv.Categories.Latest(category.Parent);
		}
		IEnumerable<CategoryPathItem>? path = parentCategory != null ? PublicationUtils.BuildPath(mcv, parentCategory).Reverse() : null;

		Store site = mcv.Stores.Latest(category.Store);
		IEnumerable<CategoryBaseModel> categories = category.Categories.Length > 0 ? LoadCategories(category.Categories) : [];

		return new CategoryModel(category)
		{
			SiteId = category.Store.ToString(),
			Path = path,
			Categories = categories,
		};
	}

	IEnumerable<CategoryBaseModel> LoadCategories(AutoId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Latest(id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}

	public IEnumerable<CategoryParentBaseModel> GetTree([NotEmpty] string siteId, [NonNegativeValue] int? depth, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {Depth}", nameof(CategoriesService), nameof(GetTree), siteId, depth);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.DepthValid(depth);

		AutoId id = AutoId.Parse(siteId);

		Store site = mcv.Stores.Latest(id);
		if(site == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), siteId);
		}

		if (site.Categories.Length == 0)
		{
			return [];
		}

		List<CategoryParentBaseModel> result = new List<CategoryParentBaseModel>(site.Categories.Length);
		LoadCategoriesRecursively(site.Categories, depth, ref result, cancellationToken);

		return result;
	}

	void LoadCategoriesRecursively(AutoId[] categories, int? currentDepth, ref List<CategoryParentBaseModel> result, CancellationToken cancellationToken)
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

			if(category.Categories.Length > 0 && (!currentDepth.HasValue || currentDepth > 1))
			{
				result.Capacity += category.Categories.Length;
				LoadCategoriesRecursively(category.Categories, currentDepth - 1, ref result, cancellationToken);
			}
		}
	}
}
