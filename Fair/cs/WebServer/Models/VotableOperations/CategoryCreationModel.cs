namespace Uccs.Fair;

public class CategoryCreationModel(CategoryCreation operation) : BaseVotableOperationModel(operation)
{
	public string ParentCategoryId { get; set; } = operation.Parent?.ToString();
	public string Title { get; set; } = operation.Title;
}
