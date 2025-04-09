using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class CategoriesController
(
	ILogger<CategoriesController> logger,
	IEntityIdValidator entityIdValidator,
	IPaginationValidator paginationValidator,
	ICategoriesService categoriesService
) : BaseController
{
	[HttpGet("{categoryId}")]
	public CategoryModel Get(string categoryId)
	{
		logger.LogInformation($"GET {nameof(CategoriesController)}.{nameof(CategoriesController.Get)} method called with {{CategoryId}}", categoryId);

		entityIdValidator.Validate(categoryId, nameof(Category).ToLower());

		return categoriesService.GetCategory(categoryId);
	}

	[HttpGet("~/api/sites/{siteId}/categories")]
	public IEnumerable<CategoryParentBaseModel> GetCategories(string siteId, [FromQuery] PaginationRequest pagination)
	{
		logger.LogInformation($"GET {nameof(CategoriesController)}.{nameof(CategoriesController.GetCategories)} method called with {{SiteId}}, {{Pagination}}", siteId, pagination);

		entityIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<CategoryParentBaseModel> categories = categoriesService.GetCategories(siteId, page, pageSize);

		return this.OkPaged(categories.Items, page, pageSize, categories.TotalItems);
	}
}
