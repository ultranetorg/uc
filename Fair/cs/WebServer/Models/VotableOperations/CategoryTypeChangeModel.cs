namespace Uccs.Fair;

public class CategoryTypeChangeModel(CategoryTypeChange operation) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; set; } = operation.Category.ToString();

	public string Type { get; set; } = operation.Type.ToString();
}
