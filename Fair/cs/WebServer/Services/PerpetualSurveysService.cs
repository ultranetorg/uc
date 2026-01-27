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

			return ToPerpetualSurveys(site.PerpetualSurveys, site.Publishers.Length);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerable<PerpetualSurveyModel> ToPerpetualSurveys(PerpetualSurvey[] surveys, int sitePublishersCount)
	{
		int id = 0;
		return surveys.Select(x => ToPerpetualSurvey<PerpetualSurveyModel, SurveyOptionModel>(sitePublishersCount, id++, x));
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
			return ToPerpetualSurvey<PerpetualSurveyDetailsModel, SurveyOptionDetailsModel>(site.Publishers.Length, surveyIndex, survey, (model, option) =>
			{
				model.YesVotes = option.Yes.Select(x => x.ToString());
			});
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static TSurvey ToPerpetualSurvey<TSurvey, TOption>(int sitePublishersCount, int id, PerpetualSurvey survey, Action<TOption, SurveyOption>? mapOption = null)
		where TOption : SurveyOptionModel, new()
		where TSurvey : BasePerpetualSurveyModel<TOption>, new()
	{
		int totalVotes = survey.Options.Sum(x => x.Yes.Length);

		var options = survey.Options.Select((x, i) =>
		{
			var newOption = new TOption
			{
				Operation = ProposalUtils.ToBaseVotableOperationModel(x.Operation),
				VotePercents = totalVotes != 0 ? (sbyte)(x.Yes.Length * 100f / totalVotes) : (sbyte)0
			};

			if(mapOption != null)
				mapOption(newOption, x);

			return newOption;
		});

		return new TSurvey
		{
			Id = id,
			LastWin = survey.LastWin,
			Options = options,
			TotalVotes = totalVotes,
			VotesRequiredToWin = sitePublishersCount / 2 + (sitePublishersCount & 1)
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
