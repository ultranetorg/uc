namespace Uccs.Fair;

public class CategoryCreationModel(CategoryCreation operation, Category? category) : BaseVotableOperationModel(operation)
{
	public string ParentCategoryId { get; } = category?.Id.ToString();
	public string ParentCategoryTitle { get; } = category?.Title;

	public string Title { get; } = operation.Title;
}
