namespace Uccs.Fair;

public class SiteDescriptionChangeModel(SiteDescriptionChange operation) : BaseVotableOperationModel(operation)
{
	public string Description { get; set; } = operation.Description;
}
