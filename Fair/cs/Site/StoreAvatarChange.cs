namespace Uccs.Fair;

public class StoreAvatarChange : VotableOperation
{
	public AutoId				File { get; set; }

	public override string		Explanation => $"Store={Store}, File={File}";
	
	public StoreAvatarChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		File = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(File);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as StoreAvatarChange;

		return o.Store == Store;
	}

 	public override bool ValidateProposal(FairExecution execution, out string error)
 	{
		if(!FileExists(execution, File, out var f, out error))
			return false;

		if(!IsImage(f, out error))
			return false;

		if(f.Owner.Id != Store.Id || (FairTable)f.Owner.Table != FairTable.Store)
		{
			error = DoesNotBelogToStore;
			return false;
		}

		return true;
 	}

	public override void Execute(FairExecution execution)
	{
		var f = execution.Files.Affect(File);
			
		Store.Avatar = f.Id;
		f.Refs++;
	}
}
