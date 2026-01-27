using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

[Route("api/author/sites/{siteId}/perpetual-surveys")]
public class AuthorPerpetualSurveysController
(
	ILogger<AuthorPerpetualSurveysController> logger,
	IAutoIdValidator autoIdValidator,
	PerpetualSurveysService proposalsService
	//IPaginationValidator paginationValidator
) : BaseController
{
	[HttpGet]
	public IEnumerable<PerpetualSurveyModel> Get(string siteId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.Get), siteId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());

		return proposalsService.GetPerpetualReferendums(siteId, cancellationToken);
	}

	[HttpGet("{perpetualSurveyId}")]
	public PerpetualSurveyDetailsModel GetDetails(string siteId, int perpetualSurveyId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {PerpetualSurveyId}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.GetDetails), siteId, perpetualSurveyId);

		autoIdValidator.Validate(siteId, nameof(Site).ToLower());
		ValidatePerpetualSurveyId(perpetualSurveyId);

		return proposalsService.GetPerpetualReferendumDetails(siteId, perpetualSurveyId);
	}

	//[HttpGet("{perpetualSurveyId}/comments")]
	//public IEnumerable<ProposalCommentModel> GetComments(string siteId, int perpetualSurveyId, [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
	//{
	//	logger.LogInformation("GET {ControllerName}.{MethodName} method called with {SiteId}, {PerpetualSurveyId}, {Pagination}", nameof(AuthorPerpetualSurveysController), nameof(AuthorPerpetualSurveysController.GetComments), siteId, perpetualSurveyId, pagination);

	//	autoIdValidator.Validate(siteId, nameof(Site).ToLower());
	//	ValidatePerpetualSurveyId(perpetualSurveyId);
	//	paginationValidator.Validate(pagination);

	//	(int page, int pageSize) = PaginationUtils.GetPaginationParams(pagination);
	//	var comments = proposalsService.GetPerpetualReferendumComments(siteId, perpetualSurveyId, page, pageSize, cancellationToken);

	//	return this.OkPaged(comments.Items, page, pageSize, comments.TotalItems);
	//}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static void ValidatePerpetualSurveyId(int perpetualSurveyId)
	{
		if(perpetualSurveyId < 0)
		{
			throw new EntityNotFoundException(EntityNames.PerpetualSurveyName, perpetualSurveyId);
		}
	}
}
