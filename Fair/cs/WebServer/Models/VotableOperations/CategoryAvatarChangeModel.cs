namespace Uccs.Fair;

public class CategoryAvatarChangeModel(CategoryAvatarChange operation) : BaseVotableOperationModel(operation)
{
	public string CategoryId { get; set; } = operation.Category.ToString();
	public AutoId File { get; set; } = operation.File;
}
