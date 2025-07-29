namespace Uccs.Fair;

public class AuthorAvatarChange : FairOperation
{
	public AutoId				Author { get; set; } 
	public AutoId				File { get; set; }

	public override string		Explanation => $"Author={Author}, File={File}";
	
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
		File	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Author);
		writer.Write(File);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAuthor(execution, Author, out var a, out Error))
			return;

		if(!CanAccessFile(execution, File, new EntityAddress(FairTable.Author, Author), out var f, out Error))
			return;

		a = execution.Authors.Affect(Author);
		f = execution.Files.Affect(File);
			
		a.Avatar = File;
		f.Refs++;

		execution.PayCycleEnergy(a);
	}
}
