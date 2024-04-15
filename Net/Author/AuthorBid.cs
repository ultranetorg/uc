using System.IO;

namespace Uccs.Net
{
	public class AuthorBid : Operation//, IEquatable<AuthorBid>
	{
		public string			Author;
		public Money			Bid;
		public override string	Description => $"{Bid} UNT for {Author}";
		
		public override bool Valid
		{
			get
			{
				if(!Transaction.Zone.Auctions)
					return false;

				if(!Net.Author.IsExclusive(Author))
					return false;

				if(Bid <= Money.Zero)
					return false;

				return true;
			}
		} 

		public AuthorBid()
		{
		}

		public AuthorBid(string name,  Money bid)
		{
			Author = name;
			Bid = bid;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Author	= reader.ReadUtf8();
			Bid		= reader.Read<Money>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Author);
			writer.Write(Bid);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Author);
			writer.Write(Bid);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new Transaction();
			
			Transaction.Signer	= reader.ReadAccount();
			Author				= reader.ReadUtf8();
			Bid					= reader.Read<Money>();
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var a = Affect(round, Author);

 			if(!Net.Author.IsExpired(a, round.ConsensusTime))
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
