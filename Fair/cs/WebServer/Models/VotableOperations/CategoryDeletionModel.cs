namespace Uccs.Fair;

public class CategoryDeletionModel(CategoryDeletion operation) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; set; } = operation.Category.ToString();
}
