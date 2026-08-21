using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class CategoriesController
(
#if DEBUG
	ILogger<CategoriesController> logger,
#endif
	CategoriesService categoriesService
) : BaseController
{
	[HttpGet("~/api/stores/{storeId}/categories/root")]
	public IEnumerable<CategoryBaseModel> GetRoot(string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(CategoriesController), nameof(CategoriesController.GetRoot), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return categoriesService.GetRoot(storeId, cancellationToken);
	}

	[HttpGet("{categoryId}")]
	public CategoryModel GetDetails(string categoryId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {CategoryId}", nameof(CategoriesController), nameof(CategoriesController.GetDetails), categoryId);
#endif

		AutoIdValidator.Validate(categoryId, nameof(Category).ToLower());

		return categoriesService.GetDetails(categoryId, cancellationToken);
	}

	[HttpGet("~/api/stores/{storeId}/categories/tree")]
	public IEnumerable<CategoryParentBaseModel> GetTree(string storeId, [FromQuery] int? depth, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Depth}", nameof(CategoriesController), nameof(CategoriesController.GetTree), storeId, depth);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		DepthValidator.Validate(depth);

		int? categoriesDepth = DepthUtils.GetDepth(depth);
		return categoriesService.GetTree(storeId, categoriesDepth, cancellationToken);
	}
}
