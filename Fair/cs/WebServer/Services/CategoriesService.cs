using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public class CategoriesService
(
#if DEBUG
	ILogger<CategoriesService> logger,
#endif
	FairMcv mcv
)
{
	public IEnumerable<CategoryBaseModel> GetRoot([NotNull][NotEmpty] string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(storeId);

		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(CategoriesService), nameof(CategoriesService.GetRoot), storeId);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if (store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if (store.Categories.Length == 0)
		{
			return [];
		}

		return LoadCategories(store.Categories);
	}

	public CategoryModel GetDetails([NotNull][NotEmpty] string categoryId, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(categoryId);

		logger.LogDebug("{ClassName}.{MethodName} method called with {CategoryId}", nameof(CategoriesService), nameof(CategoriesService.GetDetails), categoryId);
#endif

		AutoId id = AutoId.Parse(categoryId);

		Category category = mcv.Categories.Latest(id);
		if (category == null)
		{
			throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
		}

		IEnumerable<CategoryBaseModel> subCategories = category.Categories.Length > 0 ? LoadCategories(category.Categories) : [];
		Tuple<ProductType, IEnumerable<CategoryPathItem>> typeAndPath = GetCategoryTypeAndPath(category.Parent, category.Type);

		return new CategoryModel(category)
		{
			StoreId = category.Store.ToString(),
			Categories = subCategories,
			Type = typeAndPath.Item1,
			Path = typeAndPath.Item2
		};
	}

	Tuple<ProductType, IEnumerable<CategoryPathItem>> GetCategoryTypeAndPath(AutoId? parentCategoryId, ProductType categoryType)
	{
		ProductType productType = categoryType;
		List<CategoryPathItem> path = new List<CategoryPathItem>(10);

		while (parentCategoryId != null)
		{
			Category parentCategory = mcv.Categories.Latest(parentCategoryId);
			path.Add(new CategoryPathItem
			{
				Id = parentCategory.Id.ToString(),
				Title = parentCategory.Title
			});

			if (productType == ProductType.None && parentCategory.Type != ProductType.None)
			{
				productType = parentCategory.Type;
			}

			parentCategoryId = parentCategory.Parent;
		}

		path.Reverse();
		return Tuple.Create(productType, path.AsEnumerable());
	}

	IEnumerable<CategoryBaseModel> LoadCategories(AutoId[] categoriesIds)
	{
		return categoriesIds.Select(id =>
		{
			Category category = mcv.Categories.Latest(id);
			return new CategoryBaseModel(category);
		}).ToArray();
	}

	public IEnumerable<CategoryParentBaseModel> GetTree([NotEmpty] string storeId, [NonNegativeValue] int? depth, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		DepthValidator.ValidateResolved(depth);

		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Depth}", nameof(CategoriesService), nameof(CategoriesService.GetTree), storeId, depth);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		if (store.Categories.Length == 0)
		{
			return [];
		}

		List<CategoryParentBaseModel> result = new List<CategoryParentBaseModel>(store.Categories.Length);
		LoadCategoriesRecursively(store.Categories, depth, ref result, cancellationToken);

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
