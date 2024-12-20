namespace Uccs.Fair
{
	public enum PublisherAction
	{
		None, Renew, Transfer
	}

	public class AuthorUpdation : FairOperation
	{
		public new EntityId			Id {get; set;}
		public PublisherAction		Action  {get; set;}
		public byte					Years {get; set;}
		public AccountAddress		Owner  {get; set;}

		//public bool				Exclusive => Publisher.IsWeb(Address); 
		public override string		Description => $"{Id} for {Years} years";
		
		public AuthorUpdation()
		{
		}
		
		public override bool IsValid(Mcv mcv)
		{ 
			if(!Enum.IsDefined(Action) || Action == PublisherAction.None) 
				return false;
			
			if(	(Action == PublisherAction.Renew) && 
				(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
				return false;

			return true;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Id		= reader.Read<EntityId>();
			Action	= (PublisherAction)reader.ReadByte();

			if(Action == PublisherAction.Renew)
				Years = reader.ReadByte();

			if(Action == PublisherAction.Transfer)
				Owner = reader.Read<AccountAddress>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write((byte)Action);

			if(Action == PublisherAction.Renew)
				writer.Write(Years);

			if(Action == PublisherAction.Transfer)
				writer.Write(Owner);
		}

		public override void Execute(FairMcv mcv, FairRound round)
		{
			var e = mcv.Authors.Find(Id, round.Id);
			
			if(e == null)
			{
				Error = NotFound;
				return;
			}			

			if(Action == PublisherAction.Renew)
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

			if(Action == PublisherAction.Transfer)
			{
				if(!Author.IsOwner(e, Signer, round.ConsensusTime))
				{
					Error = NotOwner;
					return;
				}

				e = round.AffectAuthor(Id);
				e.Owner	= round.AffectAccount(Owner).Id;
			}
		}
	}
}
