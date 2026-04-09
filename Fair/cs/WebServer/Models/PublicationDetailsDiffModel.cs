namespace Uccs.Fair;

public class PublicationDetailsDiffModel : PublicationDetailsModel
{
	public IEnumerable<FieldValueModel>? FieldsTo { get; init; }
}
