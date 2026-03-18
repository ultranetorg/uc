namespace Uccs.Fair;

public class CategoryDeletionModel(CategoryDeletion operation, Category category) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;
}
