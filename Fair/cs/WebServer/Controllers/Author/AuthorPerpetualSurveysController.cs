using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/author/stores/{storeId}/perpetual-surveys")]
public class AuthorPerpetualSurveysController
(
	ILogger<AuthorPerpetualSurveysController> logger,
	AutoIdValidator autoIdValidator,
	PerpetualSurveysService proposalsService
) : BaseController
{
	[HttpGet]
	public IEnumerable<PerpetualSurveyModel> Get(string storeId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.Get), storeId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());

		return proposalsService.GetPerpetualReferendums(storeId, cancellationToken);
	}

	[HttpGet("{perpetualSurveyId}")]
	public PerpetualSurveyDetailsModel GetDetails(string storeId, int perpetualSurveyId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {PerpetualSurveyId}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.GetDetails), storeId, perpetualSurveyId);

		autoIdValidator.Validate(storeId, nameof(Store).ToLower());
		ValidatePerpetualSurveyId(perpetualSurveyId);

		return proposalsService.GetPerpetualReferendumDetails(storeId, perpetualSurveyId);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void ValidatePerpetualSurveyId(int perpetualSurveyId)
	{
		if(perpetualSurveyId < 0)
		{
			throw new EntityNotFoundException(EntityNames.PerpetualSurveyName, perpetualSurveyId);
		}
	}
}

//[HttpGet("{perpetualSurveyId}/comments")]
//public IEnumerable<ProposalCommentModel> GetComments(string storeId, int perpetualSurveyId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
//{
//	logger.LogInformation("GET {ControllerName}.{ActionName} method called with {StoreId}, {PerpetualSurveyId}, {Pagination}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.GetComments), storeId, perpetualSurveyId, pagination);

//	autoIdValidator.Validate(storeId, nameof(Store).ToLower());
//	ValidatePerpetualSurveyId(perpetualSurveyId);
//	paginationValidator.Validate(pagination);

//	(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
//	var comments = proposalsService.GetPerpetualReferendumComments(storeId, perpetualSurveyId, page, pageSize, cancellationToken);

//	return this.OkPaged(comments.Items, page, pageSize, comments.TotalItems);
//}
