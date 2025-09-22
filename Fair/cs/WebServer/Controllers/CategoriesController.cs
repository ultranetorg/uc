using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class CategoriesController
(
	ILogger<CategoriesController> logger,
	IAutoIdValidator autoIdValidator,
	IDepthValidator depthValidator,
	ICategoriesService categoriesService,
	ISearchQueryValidator searchQueryValidator
) : BaseController
{
	[HttpGet("{categoryId}")]
	public CategoryModel Get(string categoryId, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(CategoriesController)}.{nameof(CategoriesController.Get)} method called with {{CategoryId}}", categoryId);

		autoIdValidator.Validate(categoryId, nameof(Category).ToLower());

		return categoriesService.GetCategory(categoryId, cancellationToken);
	}

	[HttpGet("~/api/sites/{siteId}/categories")]
	public IEnumerable<CategoryParentBaseModel> GetCategories(string siteId, [FromQuery] int? depth, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(CategoriesController)}.{nameof(CategoriesController.GetCategories)} method called with {{SiteId}}, {{Depth}}", siteId, depth);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		depthValidator.Validate(depth);

		int? categoriesDepth = DepthUtils.GetDepth(depth);
		return categoriesService.GetCategories(siteId, categoriesDepth, cancellationToken);
	}
}
