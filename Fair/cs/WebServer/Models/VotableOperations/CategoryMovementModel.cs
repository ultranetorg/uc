namespace Uccs.Fair;

public class CategoryMovementModel(CategoryMovement operation, Category category, Category? parentCategory) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;

	public string? ParentCategoryId { get; } = parentCategory?.Id.ToString();
	public string? ParentCategoryTitle { get; } = parentCategory?.Title;
}
