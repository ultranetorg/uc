namespace Uccs.Fair;

public class AuthorAvatarChange : FairOperation
{
	public AutoId				Author { get; set; } 
	public byte[]				Image { get; set; }

	public override string		Explanation => $"Image={Image?.Length}";
	
	public AuthorAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Author	= reader.Read<AutoId>();
		Image	= reader.ReadBytes();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.WriteBytes(Image);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		a = execution.Authors.Affect(Author);
			
		var f = execution.AllocateFile(a.Id, a.Avatar, a, a, Image);

		a.Avatar = f?.Id;

		execution.PayCycleEnergy(a);
	}
}
