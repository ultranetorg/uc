namespace Uccs.Fair;

public class CategoryMovementModel(CategoryMovement operation, Category category, Category? parentCategory) : BaseVotableOperationModel(operation)
{
	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;

	public AutoId? ParentCategoryId { get; } = parentCategory?.Id;
	public string? ParentCategoryTitle { get; } = parentCategory?.Title;
}
