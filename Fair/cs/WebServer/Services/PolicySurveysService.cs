using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PolicySurveysService
(
#if DEBUG
	ILogger<PolicySurveysService> logger,
#endif
	FairMcv mcv
)
{
	public IEnumerable<SurveyModel> GetAll([NotNull][NotEmpty] string storeId, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(PolicySurveysService), nameof(PolicySurveysService.GetAll), storeId);
#endif

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		IEnumerable<Survey> filteredSurveys = store.Surveys.Where(x => x.Options[0].Operation is not StoreModeratorRemoval);

		return ToPerpetualSurveys(filteredSurveys, store.Publishers.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerable<SurveyModel> ToPerpetualSurveys(IEnumerable<Survey> surveys, int storePublishersCount)
	{
		return surveys.Select(x => ToPerpetualSurvey<SurveyModel, SurveyOptionModel>(storePublishersCount, x));
	}

	public SurveyDetailsModel GetDetails([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string surveyId)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(surveyId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {SurveyId}", nameof(PolicySurveysService), nameof(PolicySurveysService.GetDetails), storeId, surveyId);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		AutoId surveyEntityId = AutoId.Parse(surveyId);
		Survey survey = store.Surveys.FirstOrDefault(x => x.Id == surveyEntityId);
		if (survey == null)
		{
			throw new EntityNotFoundException(nameof(Survey).ToLower(), surveyId);
		}

		return ToPerpetualSurvey<SurveyDetailsModel, SurveyOptionDetailsModel>(store.Publishers.Length, survey, (model, option) =>
		{
			model.YesVotes = option.Yes;
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	TSurvey ToPerpetualSurvey<TSurvey, TOption>(int storePublishersCount, Survey survey, Action<TOption, SurveyOption>? mapOption = null)
		where TOption : SurveyOptionModel, new()
		where TSurvey : BaseSurveyModel<TOption>, new()
	{
		int totalVotes = survey.Options.Sum(x => x.Yes.Length);

		var options = survey.Options.Select((x, i) =>
		{
			var newOption = new TOption
			{
				Operation = ProposalUtils.ToBaseVotableOperationModel(mcv, x.Operation),
				VotePercents = totalVotes != 0 ? (sbyte)(x.Yes.Length * 100f / totalVotes) : (sbyte) 0,
				Yes = x.Yes
			};

			if(mapOption != null)
				mapOption(newOption, x);

			return newOption;
		});

		return new TSurvey
		{
			Id = survey.Id,
			LastWin = survey.LastWin,
			Options = options,
			TotalVotes = totalVotes,
			VotesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinPerpetualSurvey(storePublishersCount)
		};
	}

	public TotalItemsResult<ProposalCommentModel> GetComments([NotNull][NotEmpty] string storeId, [NonNegativeValue] int surveyIndex, [NonNegativeValue] int page, [NonZeroValue][NonNegativeValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(surveyIndex);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {SurveyIndex}, {Page}, {PageSize}", nameof(PolicySurveysService), nameof(PolicySurveysService.GetComments), storeId, surveyIndex, page, pageSize);
#endif

		AutoId entityId = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(entityId);
		
		if(store == null)
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		
		if(surveyIndex >= store.Surveys.Count)
			throw new EntityNotFoundException(nameof(EntityNames.PerpetualSurveyName).ToLower(), surveyIndex);

		Survey survey = store.Surveys[surveyIndex];
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

			string creatorName = null;
			AutoId creatorAvatarId = null;
			if(comment.As == Role.Moderator)
			{
				FairUser account = (FairUser) mcv.Users.Latest(comment.Creator);
				creatorName = account.Name;
				creatorAvatarId = comment.Creator;
			}
			else if (comment.As == Role.Publisher)
			{
				Author author = mcv.Authors.Latest(comment.Creator);
				creatorName = author.Name;
				creatorAvatarId = author.Avatar;
			}
			else
			{
				throw new InvalidOperationException($"Unexpected comment role: {comment.As}");
			}

			ProposalCommentModel model = new ProposalCommentModel
			{
				Id = comment.Id,
				Text = comment.Text,
				CreatorId = comment.Creator,
				CreatorName = creatorName,
				CreatorAs = comment.As,
				CreatorAvatarId = creatorAvatarId,
				Created = comment.Created,
				ProposalId = comment.Proposal
			};
			items.Add(model);
		}

		return new TotalItemsResult<ProposalCommentModel>
		{
			TotalItems = totalItems,
			Items = items
		};
	}
}
