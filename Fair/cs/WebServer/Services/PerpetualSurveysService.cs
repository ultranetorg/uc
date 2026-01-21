using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Ardalis.GuardClauses;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PerpetualSurveysService
(
	ILogger<PerpetualSurveysService> logger,
	FairMcv mcv
)
{
	public IEnumerable<PerpetualSurveyModel> GetPerpetualReferendums([NotNull][NotEmpty] string siteId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendums), siteId);

		Guard.Against.NullOrEmpty(siteId);

		AutoId entityId = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(entityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return ToPerpetualSurveys(site.PerpetualSurveys);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerable<PerpetualSurveyModel> ToPerpetualSurveys(PerpetualSurvey[] surveys)
	{
		int id = 0;
		return surveys.Select(x => ToPerpetualSurvey<PerpetualSurveyModel>(id++, x));
	}

	public PerpetualSurveyDetailsModel GetPerpetualReferendumDetails([NotNull][NotEmpty] string siteId, [NonNegativeValue] int surveyIndex)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {SurveyIndex}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendumDetails), siteId, surveyIndex);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(surveyIndex);

		AutoId entityId = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(entityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if (surveyIndex >= site.PerpetualSurveys.Length)
			{
				throw new EntityNotFoundException(nameof(EntityNames.PerpetualSurveyName).ToLower(), surveyIndex);
			}

			PerpetualSurvey survey = site.PerpetualSurveys[surveyIndex];
			return ToPerpetualSurvey<PerpetualSurveyDetailsModel>(surveyIndex, survey);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static T ToPerpetualSurvey<T>(int id, PerpetualSurvey survey) where T : PerpetualSurveyModel, new()
	{
		var options = survey.Options.Select(x => new SurveyOptionModel
		{
			Operation = ProposalUtils.ToBaseVotableOperationModel(x.Operation)
		});

		return new T
		{
			Id = id,
			LastWin = survey.LastWin,
			Options = options
		};
	}

	public TotalItemsResult<ProposalCommentModel> GetPerpetualReferendumComments([NotNull][NotEmpty] string siteId, [NonNegativeValue] int surveyIndex, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {SiteId}, {SurveyIndex}, {Page}, {PageSize}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendumComments), siteId, surveyIndex, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(surveyIndex);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId entityId = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(entityId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}
			if(surveyIndex >= site.PerpetualSurveys.Length)
			{
				throw new EntityNotFoundException(nameof(EntityNames.PerpetualSurveyName).ToLower(), surveyIndex);
			}

			PerpetualSurvey survey = site.PerpetualSurveys[surveyIndex];
			if(survey.Comments == null)
				return TotalItemsResult<ProposalCommentModel>.Empty;

			var comments = survey.Comments.Skip(page * pageSize).Take(pageSize);
			return LoadProposalComments(comments, survey.Comments.Length, pageSize, cancellationToken);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	TotalItemsResult<ProposalCommentModel> LoadProposalComments(IEnumerable<AutoId> commentsIds, int totalItems, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<ProposalCommentModel>.Empty;

		var items = new List<ProposalCommentModel>(pageSize);
		foreach(AutoId commentId in commentsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				break;

			ProposalComment comment = mcv.ProposalComments.Latest(commentId);
			FairUser account = (FairUser)mcv.Users.Latest(comment.Creator);
			ProposalCommentModel model = new ProposalCommentModel(comment, account);
			items.Add(model);
		}

		return new TotalItemsResult<ProposalCommentModel>
		{
			TotalItems = totalItems,
			Items = items
		};
	}
}
