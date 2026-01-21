namespace Uccs.Fair;

public class PerpetualSurveyModel
{
	public int Id { get; init; }

	public IEnumerable<SurveyOptionModel> Options { get; init; }
	public sbyte LastWin { get; init; }
}
