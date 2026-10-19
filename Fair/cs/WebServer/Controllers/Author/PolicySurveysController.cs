using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/author/stores/{storeId}/surveys/policies")]
public class PolicySurveysController
(
#if DEBUG
	ILogger<PolicySurveysController> logger,
#endif
	PolicySurveysService policySurveysService
) : BaseController
{
	[HttpGet]
	public IEnumerable<SurveyModel> Get(string storeId, CancellationToken cancellationToken)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(PolicySurveysController), nameof(PolicySurveysController.Get), storeId);
#endif

		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return policySurveysService.GetPerpetualReferendums(storeId, cancellationToken);
	}

	[HttpGet("{surveyId}")]
	public SurveyDetailsModel GetDetails(string storeId, string surveyId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {SurveyId}", nameof(PolicySurveysController), nameof(PolicySurveysController.GetDetails), storeId, surveyId);
#endif
		
		AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
		AutoIdValidator.Validate(surveyId, nameof(Survey).ToLower());

		return policySurveysService.GetPerpetualReferendumDetails(storeId, surveyId);
	}
}

//[HttpGet("{perpetualSurveyId}/comments")]
//public IEnumerable<ProposalCommentModel> GetComments(string storeId, int perpetualSurveyId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
//{
//	logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {PerpetualSurveyId}, {Pagination}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.GetComments), storeId, perpetualSurveyId, pagination);

//	AutoIdValidator.Validate(storeId, nameof(Store).ToLower());
//	ValidatePerpetualSurveyId(perpetualSurveyId);
//	paginationValidator.Validate(pagination);

//	(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
//	var comments = proposalsService.GetPerpetualReferendumComments(storeId, perpetualSurveyId, page, pageSize, cancellationToken);

//	return this.OkPaged(comments.Items, page, pageSize, comments.TotalItems);
//}
