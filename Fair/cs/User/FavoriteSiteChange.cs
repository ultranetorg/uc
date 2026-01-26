namespace Uccs.Fair;

public class FavoriteSiteChange : FairOperation
{
	public AutoId				Site { get; set; }
	public bool					Action {get; set;} /// True = Add

	public override string		Explanation => $"{Site}, AddRevoce={Action}";
	
	public FavoriteSiteChange ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		return true;
	}

	public override void Read(BinaryReader reader)
	{
		Site = reader.Read<AutoId>();
		Action = reader.ReadBoolean();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Site);
		writer.Write(Action);
	}

	public override void Execute(FairExecution execution)
	{
		if(!CanAccessAccount(execution, User.Id, out var a, out Error))
			return;

		if(Action)
		{
			if(User.FavoriteSites.Contains(Site))
			{
				Error = AlreadyExists;
				return;
			}

			User.FavoriteSites = [..User.FavoriteSites, Site];
		} 
		else
		{
			User.FavoriteSites = User.FavoriteSites.Remove(Site);
		}

		execution.PayOperationEnergy(User);
	}
}
