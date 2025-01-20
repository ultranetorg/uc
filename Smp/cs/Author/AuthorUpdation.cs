namespace Uccs.Smp;

public enum AuthorAction
{
	None, Renew, Transfer
}

public class AuthorUpdation : SmpOperation
{
	public new EntityId			Id {get; set;}
	public AuthorAction			Action  {get; set;}
	public byte					Years {get; set;}
	public AccountAddress		Owner  {get; set;}

	//public bool				Exclusive => Publisher.IsWeb(Address); 
	public override string		Description => $"{Id} for {Years} years";
	
	public AuthorUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Action) || Action == AuthorAction.None) 
			return false;
		
		if(	(Action == AuthorAction.Renew) && 
			(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Action	= (AuthorAction)reader.ReadByte();

		if(Action == AuthorAction.Renew)
			Years = reader.ReadByte();

		if(Action == AuthorAction.Transfer)
			Owner = reader.Read<AccountAddress>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write((byte)Action);

		if(Action == AuthorAction.Renew)
			writer.Write(Years);

		if(Action == AuthorAction.Transfer)
			writer.Write(Owner);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		var e = mcv.Authors.Find(Id, round.Id);
		
		if(e == null)
		{
			Error = NotFound;
			return;
		}			

		if(Action == AuthorAction.Renew)
		{	
			if(!Author.CanRenew(e, Signer, round.ConsensusTime))
			{
				Error = NotAvailable;
				return;
			}

			e = round.AffectAuthor(Id);
			e.SpaceReserved	= e.SpaceUsed;
			e.Expiration = e.Expiration + Time.FromYears(Years);
				
			PayForSpacetime(e.SpaceUsed, Time.FromYears(Years));
		}

		if(Action == AuthorAction.Transfer)
		{
			if(!Author.IsOwner(e, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			e = round.AffectAuthor(Id);
			e.Owner	= round.AffectAccount(Owner).Id;
		}
	}
}
