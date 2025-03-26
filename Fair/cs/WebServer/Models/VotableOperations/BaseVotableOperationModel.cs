namespace Uccs.Fair;

public abstract class BaseVotableOperationModel
{
	public string SignerId { get; set; }

	public BaseVotableOperationModel(VotableOperation operation)
	{
		SignerId = operation.Signer.Id.ToString();
	}
}
