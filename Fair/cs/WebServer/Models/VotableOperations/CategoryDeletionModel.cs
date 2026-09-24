namespace Uccs.Fair;

public class CategoryDeletionModel(CategoryDeletion operation, Category category) : BaseVotableOperationModel(operation)
{
	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;
}
