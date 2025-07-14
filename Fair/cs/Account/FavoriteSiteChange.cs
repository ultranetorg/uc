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
		if(!CanAccessAccount(execution, Signer.Id, out var a, out Error))
			return;

		var e = execution.AffectAccount(Signer.Id);

		if(Action)
		{
			Signer.FavoriteSites = [..Signer.FavoriteSites, Site];
		} 
		else
		{
			Signer.FavoriteSites = Signer.FavoriteSites.Remove(Site);
		}

		execution.PayCycleEnergy(Signer);
	}
}
