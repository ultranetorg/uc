namespace Uccs.Fair;

public abstract class BasePerpetualSurveyModel<T> where T : SurveyOptionModel
{
	public int Id { get; init; }

	public IEnumerable<T> Options { get; init; }
	public sbyte LastWin { get; init; }

	public int TotalVotes { get; init; }
	public int VotesRequiredToWin { get; init; }
}
