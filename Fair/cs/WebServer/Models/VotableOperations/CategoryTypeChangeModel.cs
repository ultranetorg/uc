namespace Uccs.Fair;

public class CategoryTypeChangeModel(CategoryTypeChange operation, Category category) : BaseVotableOperationModel(operation)
{
	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;
	public string CategoryType { get; } = category.Type.ToString();

	public string Type { get; set; } = operation.Type.ToString();
}
