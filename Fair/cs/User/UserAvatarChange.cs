namespace Uccs.Fair;

public class UserAvatarChange : FairOperation
{
	/// <summary>
	///  Both == null means delete avatar
	/// </summary>

	public byte[]				Image { get; set; }
	public override string		Explanation => $"Avatar={Image?.Length}";
	
	public UserAvatarChange()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Image = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteBytes(Image);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAccount(execution, User.Id, out var a, out Error))
			return;

		User.Avatar = Image;

		///execution.AllocateForever(User, Image.Length);
		execution.PayOperationEnergy(User);
	}
}
