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
	IEnumerable<SurveyModel> ToPerpetualSurveys(IEnumerable<Survey> surveys, int storePublishersCount)
	{
		return surveys.Select(x => ToPerpetualSurvey<SurveyModel, SurveyOptionModel>(storePublishersCount, x));
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

		FairOperationClass operationClass = OperationClassUtils.GetOperationClass(survey);
		bool isVariableApproval = Store.Restrictions.First(i => i.OperationClass == operationClass).Flags.HasFlag(PolicyFlag.VariableApproval);

		return new TSurvey
		{
			Id = survey.Id,
			LastWin = survey.LastWin,
			Options = options,
			TotalVotes = totalVotes,
			VotesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinPerpetualSurvey(storePublishersCount),
			IsVariableApproval = isVariableApproval
		};
	}
}
