using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/moderator/sites/{siteId}/users")]
public class ModeratorUsersController
(
	ILogger<ModeratorUsersController> logger,
	IAutoIdValidator autoIdValidator,
	IPaginationValidator paginationValidator,
	ModeratorProposalsService moderatorProposalsService,
	AccountsService accountsService,
	UserNameValidator userNameValidator
) : BaseController
{
	// TODO: move to moderator proposals controller and make it more generic (for reviews, users, etc.)
	[HttpGet]
	public IEnumerable<ProposalModel> Get(string siteId, [FromQuery] PaginationRequest pagination, [FromQuery] string? search, CancellationToken cancellationToken)
	{
		logger.LogInformation($"GET {nameof(ModeratorUsersController)}.{nameof(ModeratorUsersController.Get)} method called with {{SiteId}}, {{Pagination}}, {{Search}}", siteId, pagination, search);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		paginationValidator.Validate(pagination);

		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
		TotalItemsResult<ProposalModel> reviews = moderatorProposalsService.GetUserProposalsNotOptimized(siteId, page, pageSize, search, cancellationToken);

		return this.OkPaged(reviews.Items, page, pageSize, reviews.TotalItems);
	}

	[HttpGet("/api/moderator/users/{name}")]
	public UserModel Get(string name)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} called with {Name}", nameof(ModeratorUsersController), nameof(Get), name);

		userNameValidator.Validate(name);

		return accountsService.Get(name);
	}
}
