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
	public IEnumerable<PerpetualSurveyModel> GetPerpetualReferendums([NotNull][NotEmpty] string storeId, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendums), storeId);

		Guard.Against.NullOrEmpty(storeId);

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return ToPerpetualSurveys(store.PerpetualSurveys, store.Publishers.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerable<PerpetualSurveyModel> ToPerpetualSurveys(PerpetualSurvey[] surveys, int storePublishersCount)
	{
		int id = 0;
		return surveys.Select(x => ToPerpetualSurvey<PerpetualSurveyModel, SurveyOptionModel>(storePublishersCount, id++, x));
	}

	public PerpetualSurveyDetailsModel GetPerpetualReferendumDetails([NotNull][NotEmpty] string storeId, [NonNegativeValue] int surveyIndex)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {SurveyIndex}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendumDetails), storeId, surveyIndex);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(surveyIndex);

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if (surveyIndex >= store.PerpetualSurveys.Length)
		{
			throw new EntityNotFoundException(nameof(EntityNames.PerpetualSurveyName).ToLower(), surveyIndex);
		}

		PerpetualSurvey survey = store.PerpetualSurveys[surveyIndex];
		return ToPerpetualSurvey<PerpetualSurveyDetailsModel, SurveyOptionDetailsModel>(store.Publishers.Length, surveyIndex, survey, (model, option) =>
		{
			model.YesVotes = option.Yes.Select(x => x.ToString());
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	TSurvey ToPerpetualSurvey<TSurvey, TOption>(int storePublishersCount, int id, PerpetualSurvey survey, Action<TOption, SurveyOption>? mapOption = null)
		where TOption : SurveyOptionModel, new()
		where TSurvey : BasePerpetualSurveyModel<TOption>, new()
	{
		int totalVotes = survey.Options.Sum(x => x.Yes.Length);

		var options = survey.Options.Select((x, i) =>
		{
			var newOption = new TOption
			{
				Operation = ProposalUtils.ToBaseVotableOperationModel(mcv, x.Operation),
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
			VotesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinPerpetualSurvey(storePublishersCount)
		};
	}

	public TotalItemsResult<ProposalCommentModel> GetPerpetualReferendumComments([NotNull][NotEmpty] string storeId, [NonNegativeValue] int surveyIndex, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {SurveyIndex}, {Page}, {PageSize}", nameof(PerpetualSurveysService), nameof(PerpetualSurveysService.GetPerpetualReferendumComments), storeId, surveyIndex, page, pageSize);

		Guard.Against.NullOrEmpty(storeId);
		Guard.Against.Negative(surveyIndex);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}
		if(surveyIndex >= store.PerpetualSurveys.Length)
		{
			throw new EntityNotFoundException(nameof(EntityNames.PerpetualSurveyName).ToLower(), surveyIndex);
		}

		PerpetualSurvey survey = store.PerpetualSurveys[surveyIndex];
		if(survey.Comments == null)
			return TotalItemsResult<ProposalCommentModel>.Empty;

		var comments = survey.Comments.Skip(page * pageSize).Take(pageSize);
		return LoadProposalComments(comments, survey.Comments.Length, pageSize, cancellationToken);
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
