using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class CategoriesController
(
	ILogger<CategoriesController> logger,
	AutoIdValidator autoIdValidator,
	DepthValidator depthValidator,
	CategoriesService categoriesService
) : BaseController
{
	[HttpGet("~/api/stores/{storeId}/categories/root")]
	public IEnumerable<CategoryBaseModel> GetRoot(string storeId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(CategoriesController), nameof(GetRoot), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return categoriesService.GetRoot(storeId, cancellationToken);
	}

	[HttpGet("{categoryId}")]
	public CategoryModel GetDetails(string categoryId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {CategoryId}", nameof(CategoriesController), nameof(GetDetails), categoryId);

		autoIdValidator.Validate(categoryId, nameof(Category).ToLower());

		return categoriesService.GetDetails(categoryId, cancellationToken);
	}

	[HttpGet("~/api/stores/{storeId}/categories/tree")]
	public IEnumerable<CategoryParentBaseModel> GetTree(string storeId, [FromQuery] int? depth, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {Depth}", nameof(CategoriesController), nameof(GetTree), storeId, depth);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		depthValidator.Validate(depth);

		int? categoriesDepth = DepthUtils.GetDepth(depth);
		return categoriesService.GetTree(storeId, categoriesDepth, cancellationToken);
	}
}
