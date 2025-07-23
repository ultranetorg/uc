namespace Uccs.Fair;

public class CategoryMovementModel(CategoryMovement operation) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; set; } = operation.Category.ToString();
	public string ParentCategoryId { get; set; } = operation.Parent.ToString();
}
