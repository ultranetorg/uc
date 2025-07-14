namespace Uccs.Fair;

public class AccountAvatarChange : FairOperation
{
	/// <summary>
	///  Both == null means delete avatar
	/// </summary>

	public byte[]				Avatar { get; set; }
	public override string		Explanation => $"Avatar={Avatar?.Length}";
	
	public AccountAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Avatar = reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Avatar);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAccount(execution, Signer.Id, out var a, out Error))
			return;

		Signer.Avatar = Avatar;

		execution.AllocateForever(Signer, Avatar.Length);
		execution.PayCycleEnergy(Signer);
	}
}
