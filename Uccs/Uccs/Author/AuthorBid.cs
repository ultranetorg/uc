using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorBid : Operation//, IEquatable<AuthorBid>
	{
		public string			Author;
		public Money			Bid;
		public string			Tld;
		public override string	Description => $"{Bid} UNT for {Author}{(Tld != null ? $", {Tld}" : null)}";
		
		public override bool Valid
		{
			get
			{
				if(!Net.Author.IsExclusive(Author))
					return false;

				if(Bid <= Money.Zero)
					return false;

				if(Tld.Any() && !(Tld == "com" || Tld == "org" || Tld == "net"))
					return false;

				return true;
			}
		} 

		public AuthorBid()
		{
		}

		public AuthorBid(string name, string tld, Money bid)
		{
			Author = name;
			Tld = tld ?? "";
			Bid = bid;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Author	= reader.ReadUtf8();
			Tld		= reader.ReadUtf8();
			Bid		= reader.Read<Money>();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Author);
			writer.WriteUtf8(Tld);
			writer.Write(Bid);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Author);
			writer.WriteUtf8(Tld);
			writer.Write(Bid);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_Id	= reader.Read<OperationId>();

			Transaction = new Transaction();
			
			Transaction.Signer	= reader.ReadAccount();
			Author				= reader.ReadUtf8();
			Tld					= reader.ReadUtf8();
			Bid					= reader.Read<Money>();
		}
		public override void Execute(Mcv mcv, Round round)
		{
			if(Tld.Any())
			{
				Check(mcv, round);
			} 
			else
			{
				ConsensusExecute(round);
			}
		}

		public void Check(Mcv mcv, Round round)
		{
			var a =  mcv.Authors.Find(Author, round.Id);

 			if(a != null && !Net.Author.IsExpired(a, round.ConsensusTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					return;
				}
				else if(round.ConsensusTime < a.AuctionEnd)
				{
					if((!a.DomainOwnersOnly && Tld.Any()) || (a.DomainOwnersOnly && a.LastBid < Bid && Tld.Any())) /// outbid
					{
						return;
					}
				}
 			}
 			else
 			{
				return;
			}

			Error = "Bid too low or auction is over";
		}

		public void ConsensusExecute(Round round)
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
					a.DomainOwnersOnly	= Tld.Any();
						
					return;
				}
				else if(round.ConsensusTime < a.AuctionEnd)
				{
					if((!a.DomainOwnersOnly && (a.LastBid < Bid || Tld.Any())) || (a.DomainOwnersOnly && a.LastBid < Bid && Tld.Any())) /// outbid
					{
						Affect(round, a.LastWinner).Balance += a.LastBid;
						Affect(round, Signer).Balance -= Bid;
						
						if(!a.DomainOwnersOnly && Tld.Any())
							a.DomainOwnersOnly = true;
						
						a.LastBid		= Bid;
						a.LastBidTime	= round.ConsensusTime;
						a.LastWinner	= Signer;
				
						return;
					}
				}
 			}
 			else
 			{
				if(a.Owner != null)
				{
					a.Owner = null;
				}

				/// dont refund previous winner
				Fee += a.LastBid;

				Affect(round, Signer).Balance -= Bid;
				
				a.Owner				= null;
				a.FirstBidTime		= round.ConsensusTime;
				a.LastBid			= Bid;
				a.LastBidTime		= round.ConsensusTime;
				a.LastWinner		= Signer;
				a.DomainOwnersOnly	= Tld.Any();
			
				return;
			}

			Error = "Bid too low or auction is over";
		}

// 		public override bool Equals(object obj)
// 		{
// 			return Equals(obj as AuthorBid);
// 		}
// 
// 		public bool Equals(AuthorBid a)
// 		{
// 			return a is not null && Id.Equals(a.Id) && Signer == a.Signer && Name == a.Name && Bid.Equals(a.Bid) && Tld == a.Tld;
// 		}
	}
}
