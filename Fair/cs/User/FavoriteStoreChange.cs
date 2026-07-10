namespace Uccs.Fair;

public class FavoriteStoreChange : FairOperation
{
	public AutoId				Store { get; set; }
	public bool					Action {get; set;} /// True = Add

	public override string		Explanation => $"Store={Store}, AddRevoce={Action}";
	
	public FavoriteStoreChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(Reader reader)
	{
		Store = reader.Read<AutoId>();
		Action = reader.ReadBoolean();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Store);
		writer.Write(Action);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAccount(execution, User.Id, out var a, out Error))
			return;

		if(Action)
		{
			if(User.FavoriteStores.Contains(Store))
			{
				Error = AlreadyExists;
				return;
			}

			User.FavoriteStores = [..User.FavoriteStores, Store];
		} 
		else
		{
			User.FavoriteStores = User.FavoriteStores.Remove(Store);
		}

		execution.PayOperationEnergy(User);
	}
}
