namespace Uccs.Fair;

public enum AuthorChange : byte
{
	None, Renew, Transfer
}

public class AuthorUpdation : FairOperation
{
	public new EntityId			Id { get; set; }
	public AuthorChange			Action  { get; set; }
	public byte					Years { get; set; }
	public AccountAddress		Owner  { get; set; }

	//public bool				Exclusive => Publisher.IsWeb(Address); 
	public override string		Description => $"{Id} for {Years} years";
	
	public AuthorUpdation()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Enum.IsDefined(Action) || Action == AuthorChange.None) 
			return false;
		
		if(	(Action == AuthorChange.Renew) && 
			(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Id		= reader.Read<EntityId>();
		Action	= reader.ReadEnum<AuthorChange>();

		if(Action == AuthorChange.Renew)
			Years = reader.ReadByte();

		if(Action == AuthorChange.Transfer)
			Owner = reader.Read<AccountAddress>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.WriteEnum(Action);

		if(Action == AuthorChange.Renew)
			writer.Write(Years);

		if(Action == AuthorChange.Transfer)
			writer.Write(Owner);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireAuthorAccess(round, Id, out var a))
			return;

		if(Action == AuthorChange.Renew)
		{	
			if(!Author.CanRenew(a, Signer, round.ConsensusTime))
			{
				Error = NotAvailable;
				return;
			}

			a = round.AffectAuthor(Id);
			a.SpaceReserved	= a.SpaceUsed;
			a.Expiration = a.Expiration + Time.FromYears(Years);
				
			PayForSpacetime(a.SpaceUsed, Time.FromYears(Years));
		}

		if(Action == AuthorChange.Transfer)
		{
			if(!Author.IsOwner(a, Signer, round.ConsensusTime))
			{
				Error = Denied;
				return;
			}

			a = round.AffectAuthor(Id);
			a.Owner	= round.AffectAccount(Owner).Id;
		}
	}
}
