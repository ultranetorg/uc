namespace Uccs.Fair;

public class CategoryTypeChangeModel(CategoryTypeChange operation, Category category) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;
	public string CategoryType { get; } = category.Type.ToString();

	public string Type { get; set; } = operation.Type.ToString();
}
