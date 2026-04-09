using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/sites/{siteId}/products/unpublished")]
public class UnpublishedSiteProductsController
(
	ILogger<UnpublishedSiteProductsController> logger,
	IAutoIdValidator autoIdValidator,
	UnpublishedSiteProductsService unpublishedSiteProductsService
) : BaseController
{
	/// <summary>
	/// Returns Products that have not been published on the Site.
	/// </summary>
	[HttpGet("{productId}")]
	public ProductDetailsModel GetDetails(string siteId, string productId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {ProductId}", nameof(UnpublishedSiteProductsController), nameof(GetDetails), siteId, productId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		autoIdValidator.Validate(productId, nameof(Product).ToLower());

		return unpublishedSiteProductsService.GetDetails(siteId, productId);
	}
}