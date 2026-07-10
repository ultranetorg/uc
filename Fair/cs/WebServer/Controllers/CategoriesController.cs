using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class CategoriesController
(
	ILogger<CategoriesController> logger,
	IAutoIdValidator autoIdValidator,
	IDepthValidator depthValidator,
	CategoriesService categoriesService
) : BaseController
{
	[HttpGet("~/api/sites/{siteId}/categories/root")]
	public IEnumerable<CategoryBaseModel> GetRoot(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}", nameof(CategoriesController), nameof(GetRoot), siteId);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());

		return categoriesService.GetRoot(siteId, cancellationToken);
	}

	[HttpGet("{categoryId}")]
	public CategoryModel GetDetails(string categoryId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {CategoryId}", nameof(CategoriesController), nameof(GetDetails), categoryId);

		autoIdValidator.Validate(categoryId, nameof(Category).ToLower());

		return categoriesService.GetDetails(categoryId, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/categories/tree")]
	public IEnumerable<CategoryParentBaseModel> GetTree(string siteId, [FromQuery] int? depth, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {SiteId}, {Depth}", nameof(CategoriesController), nameof(GetTree), siteId, depth);

		autoIdValidator.Validate(siteId, nameof(Store).ToLower());
		depthValidator.Validate(depth);

		int? categoriesDepth = DepthUtils.GetDepth(depth);
		return categoriesService.GetTree(siteId, categoriesDepth, cancellationToken);
	}
}
