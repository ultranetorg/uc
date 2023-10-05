using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorBid : Operation//, IEquatable<AuthorBid>
	{
		public string			Name;
		public Coin				Bid;
		public string			Tld;
		public override string	Description => $"{Bid} UNT for {Name}, {Tld}";
		
		public override bool Valid
		{
			get
			{
				if(!Author.IsExclusive(Name))
					return false;

				if(Bid <= Coin.Zero)
					return false;

				if(Tld.Any() && !(Tld == "com" || Tld == "org" || Tld == "net"))
					return false;

				return true;
			}
		} 

		public AuthorBid()
		{
		}

		public AuthorBid(string name, string tld, Coin bid)
		{
			Name = name;
			Tld = tld ?? "";
			Bid = bid;
		}
		
		public override void ReadConfirmed(BinaryReader reader)
		{
			Name	= reader.ReadUtf8();
			Tld		= reader.ReadUtf8();
			Bid		= reader.ReadCoin();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Name);
			writer.WriteUtf8(Tld);
			writer.Write(Bid);
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write(Id);
			writer.Write(Signer);
			writer.WriteUtf8(Name);
			writer.WriteUtf8(Tld);
			writer.Write(Bid);
		}

		public void ReadBaseState(BinaryReader reader)
		{
			_id	= reader.Read<OperationId>();
			Signer	= reader.ReadAccount();
			Name	= reader.ReadUtf8();
			Tld		= reader.ReadUtf8();
			Bid		= reader.ReadCoin();
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
			var a =  mcv.Authors.Find(Name, round.Id);

 			if(a != null && !Author.IsExpired(a, round.ConfirmedTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					return;
				}
				else if(round.ConfirmedTime < a.AuctionEnd)
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
			var a = round.AffectAuthor(Name);

 			if(!Author.IsExpired(a, round.ConfirmedTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					round.AffectAccount(Signer).Balance -= Bid;
						
					a.Owner				= null;
					a.FirstBidTime		= round.ConfirmedTime;
					a.LastBid			= Bid;
					a.LastBidTime		= round.ConfirmedTime;
					a.LastWinner		= Signer;
					a.DomainOwnersOnly	= Tld.Any();
						
					return;
				}
				else if(round.ConfirmedTime < a.AuctionEnd)
				{
					if((!a.DomainOwnersOnly && (a.LastBid < Bid || Tld.Any())) || (a.DomainOwnersOnly && a.LastBid < Bid && Tld.Any())) /// outbid
					{
						round.AffectAccount(a.LastWinner).Balance += a.LastBid;
						round.AffectAccount(Signer).Balance -= Bid;
						
						if(!a.DomainOwnersOnly && Tld.Any())
							a.DomainOwnersOnly = true;
						
						a.LastBid		= Bid;
						a.LastBidTime	= round.ConfirmedTime;
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
				round.Fees += a.LastBid;

				round.AffectAccount(Signer).Balance -= Bid;
				
				a.Owner				= null;
				a.FirstBidTime		= round.ConfirmedTime;
				a.LastBid			= Bid;
				a.LastBidTime		= round.ConfirmedTime;
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
