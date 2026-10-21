namespace Uccs.Fair;

public class ChangedPublicationDetailsModel : ChangedPublicationModel
{
	public byte Rating { get; init; }

	public IEnumerable<FieldValueModel> Fields { get; init; }
	public IEnumerable<FieldValueModel> FieldsTo { get; init; }
}
