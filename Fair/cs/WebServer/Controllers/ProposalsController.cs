//using Microsoft.AspNetCore.Mvc;
//using Uccs.Web.Pagination;

//namespace Uccs.Fair;

//public class ProposalsController
//(
//	ILogger<ProposalsController> logger,
//	ProposalService2 proposalsService,
//	AutoIdValidator autoIdValidator,
//	PaginationValidator paginationValidator
//) : BaseController
//{
//	[HttpGet]
//	public IEnumerable<ProposalModel2> Get(string siteId, [FromQuery] FairOperationClass? operation, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
//	{
//		logger.LogInformation($"GET {nameof(ProposalsController)}.{nameof(ProposalsController.Get)} method called with {{SiteId}}, {{Operation}}, {{Pagination}}", siteId, operation, pagination);

//		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
//		paginationValidator.Validate(pagination);

//		(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
//		TotalItemsResult<ProposalModel2> discussions = proposalsService.GetProposals(siteId, operation, page, pageSize, cancellationToken);

//		return this.OkPaged(discussions.Items, page, pageSize, discussions.TotalItems);
//	}
//}
