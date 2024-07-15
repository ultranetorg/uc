namespace Uccs.Rdn
{
	public class DomainBid : RdnOperation
	{
		public string			Name;
		public Unit			Bid;
		public override string	Description => $"{Bid} UNT for {Name}";
		
		public override bool IsValid(Mcv mcv)
		{
			if(!(mcv.Zone as RdnZone).Auctions)
				return false;

			if(!Domain.IsWeb(Name))
				return false;

			if(Bid <= Unit.Zero)
				return false;

			return true;
		} 

		public DomainBid()
		{
		}

		public DomainBid(string name,  Unit bid)
		{
			Name = name;
			Bid = bid;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Name	= reader.ReadUtf8();
			Bid		= reader.Read<Unit>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Name);
			writer.Write(Bid);
		}

// 		public void WriteBaseState(BinaryWriter writer)
// 		{
// 			writer.Write(Id);
// 			writer.Write(Signer);
// 			writer.WriteUtf8(Name);
// 			writer.Write(Bid);
// 		}
// 
// 		public void ReadBaseState(BinaryReader reader)
// 		{
// 			_Id	= reader.Read<OperationId>();
// 
// 			Transaction = new Transaction();
// 			
// 			Transaction.Signer	= reader.ReadAccount();
// 			Name				= reader.ReadUtf8();
// 			Bid					= reader.Read<Money>();
// 		}

		public override void Execute(RdnMcv mcv, RdnRound round)
		{
			var a = round.AffectDomain(Name);

 			if(!Domain.IsExpired(a, round.ConsensusTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					Signer.STBalance -= Bid;
					
					a.Owner				= null;
					a.FirstBidTime		= round.ConsensusTime;
					a.LastBid			= Bid;
					a.LastBidTime		= round.ConsensusTime;
					a.LastWinner		= Signer.Id;
						
					return;
				}
				else if(round.ConsensusTime < a.AuctionEnd)
				{
					if(a.LastBid < Bid) /// outbid
					{
						var lw = mcv.Accounts.Find(a.LastWinner, round.Id);
						
						Affect(round, lw.Address).STBalance += a.LastBid;
						Signer.STBalance -= Bid;
						
						a.LastBid		= Bid;
						a.LastBidTime	= round.ConsensusTime;
						a.LastWinner	= Signer.Id;
				
						return;
					}
				}
 			}
 			else
 			{
				/// dont refund previous winner if any
				Transaction.STReward += a.LastBid;

				Signer.STBalance -= Bid;
				
				a.Owner				= null;
				a.FirstBidTime		= round.ConsensusTime;
				a.LastBid			= Bid;
				a.LastBidTime		= round.ConsensusTime;
				a.LastWinner		= Signer.Id;
			
				return;
			}

			Error = "Bid too low or auction is over";
		}
	}
}
