using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;
using Uccs.Web.Utilities;

namespace Uccs.Fair;

public class CategoriesController
(
	ILogger<CategoriesController> logger,
	IEntityIdValidator entityIdValidator,
	ICategoriesService categoriesService
) : BaseController
{
	[HttpGet("{categoryId}")]
	public CategoryModel Get(string categoryId)
	{
		logger.LogInformation($"GET {nameof(CategoriesController)}.{nameof(CategoriesController.Get)} method called with {{CategoryId}}", categoryId);

		entityIdValidator.Validate(categoryId, nameof(Category).ToLower());

		CategoryModel category = categoriesService.Find(categoryId);
		if (category == null)
		{
			throw new EntityNotFoundException(nameof(Category).ToLower(), categoryId);
		}

		return category;
	}
}
