using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorBid : Operation
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

		public AuthorBid(AccountAddress signer, string name, string tld, Coin bid)
		{
			Signer = signer;
			Name = name;
			Tld = tld ?? "";
			Bid = bid;
		}
		
		protected override void ReadConfirmed(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			Tld		= r.ReadUtf8();
			Bid		= r.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.WriteUtf8(Tld);
			w.Write(Bid);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var a = round.AffectAuthor(Name);

 			if(!Author.IsExpired(a, round.ConfirmedTime))
 			{
				if(a.LastWinner == null) /// first bid
				{
					round.AffectAccount(Signer).Balance -= Bid;
						
					a.Owner			= null;
					a.FirstBidTime	= round.ConfirmedTime;
					a.LastBid		= Bid;
					a.LastBidTime	= round.ConfirmedTime;
					a.LastWinner	= Signer;
						
					return;
				}
				else if(round.ConfirmedTime < a.AuthionEnd)
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
				
				a.Owner			= null;
				a.FirstBidTime	= round.ConfirmedTime;
				a.LastBid		= Bid;
				a.LastBidTime	= round.ConfirmedTime;
				a.LastWinner	= Signer;
			
				return;
			}

			Error = "Bid too low or auction is over";
		}

// 		public static bool CanBid(Author author, ChainTime time)
// 		{
// 			if(author == null)
// 				return true;
// 
// 			ChainTime sinceauction() => time - author.FirstBidTime;
// 
// 			bool expired = author.LastWinner != null && (	author.Owner == null && sinceauction() > ChainTime.FromYears(2) ||		/// winner has not registered during 2 year since auction start, restart the auction
// 															author.Owner != null && time > author.Expiration);	/// is not renewed by owner, restart the auction
// 
//  			if(!expired)
//  			{
// 	 			if(author.LastWinner == null || sinceauction() < ChainTime.FromYears(1))
// 	 			{
// 					return true;
// 	 			}
//  			} 
//  			else
// 				return true;
// 
// 			return false;
// 		}
	}
}
