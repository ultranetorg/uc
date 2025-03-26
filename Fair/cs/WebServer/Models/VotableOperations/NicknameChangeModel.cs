namespace Uccs.Fair;

public class NicknameChangeModel : BaseVotableOperationModel
{
	public string Nickname { get; set; }

	public EntityTextField Field { get; set; }

	public string EntityId { get; set; }

	public NicknameChangeModel(NicknameChange operation) : base(operation)
	{
		Nickname = operation.Nickname;
		Field = operation.Field;
		EntityId = operation.Entity.ToString();
	}
}
