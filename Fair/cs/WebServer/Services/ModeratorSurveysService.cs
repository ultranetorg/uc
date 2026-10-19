using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class ModeratorSurveysService
(
#if DEBUG
	ILogger<ModeratorSurveysService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<SurveyModel> GetSurveys([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(ModeratorSurveysService), nameof(ModeratorSurveysService.GetSurveys), storeId, page, pageSize);
#endif

		AutoId entityId = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(entityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		Survey[] moderatorSurveys = store.Surveys.Where(x => x.Options[0].Operation is StoreModeratorRemoval).ToArray();
		IEnumerable<Survey> filteredAndPagedSurveys = moderatorSurveys
			.Skip(page * pageSize)
			.Take(pageSize);

		return ToPerpetualSurveys(moderatorSurveys, moderatorSurveys.Length, store.Publishers.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	TotalItemsResult<SurveyModel> ToPerpetualSurveys(IEnumerable<Survey> surveys, int totalSurveysCount, int storePublishersCount)
	{
		IEnumerable<SurveyModel> items = surveys.Select(x => ToSurvey(x, storePublishersCount));
		return new TotalItemsResult<SurveyModel>
		{
			TotalItems = totalSurveysCount,
			Items = items
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	SurveyModel? ToSurvey(Survey survey, int storePublishersCount)
	{
		int totalVotes = survey.Options.Sum(x => x.Yes.Length);

		var options = survey.Options.Select((x, i) =>
			new SurveyOptionModel
			{
				Operation = ProposalUtils.ToBaseVotableOperationModel(mcv, x.Operation),
				VotePercents = totalVotes != 0 ? (sbyte)(x.Yes.Length * 100f / totalVotes) : (sbyte) 0,
				Yes = x.Yes
			}
		);

		return new SurveyModel
		{
			Id = survey.Id,
			LastWin = survey.LastWin,
			Options = options,
			TotalVotes = totalVotes,
			VotesRequiredToWin = VotingUtils.CalculateVotesRequiredToWinPerpetualSurvey(storePublishersCount),
		};
	}
}
