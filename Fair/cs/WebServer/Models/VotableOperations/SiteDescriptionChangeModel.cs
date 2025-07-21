namespace Uccs.Fair;

public class SiteDescriptionChangeModel(SiteTextChange operation) : BaseVotableOperationModel(operation)
{
	public string Description { get; set; } = operation.Description;
}
