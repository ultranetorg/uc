namespace Uccs.Fair;

public class SiteNameChangeModel(SiteNameChange operation, string name) : BaseVotableOperationModel(operation)
{
	public string SiteName { get; } = name;
	public string Name { get; } = operation.Name;
}
