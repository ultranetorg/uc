namespace Uccs.Fair;

public class CategoryCreationModel(CategoryCreation operation, Category? category) : BaseVotableOperationModel(operation)
{
	public AutoId? ParentCategoryId { get; } = category?.Id;
	public string ParentCategoryTitle { get; } = category?.Title;

	public string Title { get; } = operation.Title;
}
