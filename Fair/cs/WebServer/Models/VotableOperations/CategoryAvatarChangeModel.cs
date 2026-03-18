namespace Uccs.Fair;

public class CategoryAvatarChangeModel(CategoryAvatarChange operation, Category category) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; } = category.Id.ToString();
	public string CategoryTitle { get; } = category.Title;

	public string FileId { get; set; } = operation.File.ToString();
}
