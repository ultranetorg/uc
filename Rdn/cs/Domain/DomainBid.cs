namespace Uccs.Rdn;

public class DomainBid : RdnOperation
{
	public string			Name;
	public long				Bid;
	public override string	Description => $"{Bid} UNT for {Name}";
	
	public override bool IsValid(Mcv mcv)
	{
		if(!(mcv.Net as Rdn).Auctions)
			return false;

		if(!Domain.IsWeb(Name))
			return false;

		if(Bid <= 0)
			return false;

		return true;
	} 

	public DomainBid()
	{
	}

	public DomainBid(string name, long bid)
	{
		Name = name;
		Bid = bid;
	}
	
	public override void ReadConfirmed(BinaryReader reader)
	{
		Name	= reader.ReadUtf8();
		Bid		= reader.Read7BitEncodedInt64();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.WriteUtf8(Name);
		writer.Write7BitEncodedInt64(Bid);
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
				Signer.BYBalance -= Bid;
				
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
					
					Affect(round, lw.Address).BYBalance += a.LastBid;
					Signer.BYBalance -= Bid;
					
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
			///Transaction.BYReturned += a.LastBid;

			Signer.BYBalance -= Bid;
			
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
