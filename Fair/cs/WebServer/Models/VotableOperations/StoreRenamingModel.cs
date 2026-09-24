namespace Uccs.Fair;

public class StoreRenamingModel(StoreRenaming operation, string name) : BaseVotableOperationModel(operation)
{
	public string StoreName { get; } = name;
	public string Name { get; } = operation.Name;
}
