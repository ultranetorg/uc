namespace Uccs.Fair;

public class FieldValueModel
{
	public Token Name { get; set; }

	public FieldType? Type { get; set; }

	public object Value { get; set; }
	
	public IEnumerable<FieldValueModel> Children { get; set; }
}