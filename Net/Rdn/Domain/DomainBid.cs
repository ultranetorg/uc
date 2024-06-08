using System.IO;

namespace Uccs.Net
{
	public class DomainBid : RdnOperation
	{
		public string			Name;
		public Money			Bid;
		public override string	Description => $"{Bid} UNT for {Name}";
		
		public override bool IsValid(Mcv mcv)
		{
			if(!Transaction.Zone.Auctions)
				return false;

			if(!Domain.IsWeb(Name))
				return false;

			if(Bid <= Money.Zero)
				return false;

			return true;
		} 

		public DomainBid()
		{
		}

		public DomainBid(string name,  Money bid)
		{
			Name = name;
			Bid = bid;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Name	= reader.ReadUtf8();
			Bid		= reader.Read<Money>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Name);
			writer.Write(Bid);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Name);
			writer.Write(Bid);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new Transaction();
			
			Transaction.Signer	= reader.ReadAccount();
			Name				= reader.ReadUtf8();
			Bid					= reader.Read<Money>();
		}

		public override void Execute(Rdn mcv, RdnRound round)
		{
			var a = round.AffectDomain(Name);

 			if(!Domain.IsExpired(a, round.ConsensusTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					Affect(round, Signer).Balance -= Bid;
					
					a.Owner				= null;
					a.FirstBidTime		= round.ConsensusTime;
					a.LastBid			= Bid;
					a.LastBidTime		= round.ConsensusTime;
					a.LastWinner		= Signer;
						
					return;
				}
				else if(round.ConsensusTime < a.AuctionEnd)
				{
					if(a.LastBid < Bid) /// outbid
					{
						Affect(round, a.LastWinner).Balance += a.LastBid;
						Affect(round, Signer).Balance -= Bid;
						
						a.LastBid		= Bid;
						a.LastBidTime	= round.ConsensusTime;
						a.LastWinner	= Signer;
				
						return;
					}
				}
 			}
 			else
 			{
				/// dont refund previous winner if any
				Reward += a.LastBid;

				Affect(round, Signer).Balance -= Bid;
				
				a.Owner				= null;
				a.FirstBidTime		= round.ConsensusTime;
				a.LastBid			= Bid;
				a.LastBidTime		= round.ConsensusTime;
				a.LastWinner		= Signer;
			
				return;
			}

			Error = "Bid too low or auction is over";
		}
	}
}
