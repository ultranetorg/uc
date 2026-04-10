namespace Uccs.Fair;

public class ChangedPublicationDetailsModel : ChangedPublicationModel
{
	public int Rating { get; init; }

	public IEnumerable<FieldValueModel> Fields { get; init; }
	public IEnumerable<FieldValueModel> FieldsTo { get; init; }
}
