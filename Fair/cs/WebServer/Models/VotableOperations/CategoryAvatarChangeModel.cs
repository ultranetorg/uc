namespace Uccs.Fair;

public class CategoryAvatarChangeModel(CategoryAvatarChange operation, Category category) : BaseVotableOperationModel(operation)
{
	public AutoId CategoryId { get; } = category.Id;
	public string CategoryTitle { get; } = category.Title;

	public AutoId FileId { get; set; } = operation.File;
}
