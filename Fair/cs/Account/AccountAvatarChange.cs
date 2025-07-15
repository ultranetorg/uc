namespace Uccs.Fair;

public class AccountAvatarChange : FairOperation
{
	/// <summary>
	///  Both == null means delete avatar
	/// </summary>

	public byte[]				Image { get; set; }
	public override string		Explanation => $"Avatar={Image?.Length}";
	
	public AccountAvatarChange()
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
		if(!CanAccessAccount(execution, Signer.Id, out var a, out Error))
			return;

		Signer.Avatar = Image;

		execution.AllocateForever(Signer, Image.Length);
		execution.PayCycleEnergy(Signer);
	}
}
