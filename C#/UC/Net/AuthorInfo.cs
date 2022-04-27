using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class OperationInfo
	{
		public int			RoundId {get; set; }
		public ChainTime	RoundTime {get; set; }

		public OperationInfo(){}
		public OperationInfo(Operation o)
		{
			RoundId  = o.Transaction.Payload.Round.Id;
			RoundTime = o.Transaction.Payload.Round.Time;
		}
	}

	public class AuthorBidInfo : OperationInfo
	{
		public Coin		Amount {get; set; }
		public Account	Account {get; set; }

		public AuthorBidInfo(){}
		public AuthorBidInfo(AuthorBid b) : base(b)
		{
			Amount	= b.Bid;
			Account	= b.Signer;
		}
	}

	public class AuthorRegistrationInfo : OperationInfo
	{
		public Account		Account {get; set; }
		public string		Title {get; set; }
		public byte			Years {get; set; }
		public ChainTime	Till  => RoundTime + ChainTime.FromYears(Years);

		public AuthorRegistrationInfo(){}
		public AuthorRegistrationInfo(AuthorRegistration r) : base(r)
		{
			Account = r.Signer;
			Title = r.Title;
			Years = r.Years;
		}
	}

	public class AuthorTransferInfo : OperationInfo
	{
		public Account		NewOwner {get; set; }
		
		public AuthorTransferInfo(){}
		public AuthorTransferInfo(AuthorTransfer t) : base(t)
		{
			NewOwner = t.To;
		}
	}

	public class AuthorInfo
	{
		public string					Name {get; set;}
		public AuthorBidInfo			FirstBid {get; set;}
		public AuthorBidInfo			LastBid {get; set;}
		public AuthorRegistrationInfo	LastRegistration {get; set;}
		public AuthorTransferInfo		LastTransfer {get; set;}
		public Account					Owner {get; set;}
		public List<string>				Products {get; set;}

		public void Dump(Action<string, object> top, Action<string, object> sub)
		{
			void at(OperationInfo o)
			{
				sub("At", $"Round {o.RoundId}, {o.RoundTime}");
			}

			top("Name",		Name);
			top("Owner",	Owner);
			
			if(LastRegistration != null)
			{
				top("Last Registration", null);
					sub("By", LastRegistration.Account);
					sub("Title", LastRegistration.Title);
					sub("Years", LastRegistration.Years);
					sub("Till", LastRegistration.Till.ToString());
					at(LastRegistration);
			}

			if(LastTransfer != null)
			{
				top("Last Transfer", null);
					sub("To", LastTransfer.NewOwner);
					at(LastTransfer);
			}

			if(Name.Length <= AuthorRegistration.LengthMaxForAuction)
			{
				if(FirstBid != null)
				{
					top("First Bid", null);
						sub("Amount",	FirstBid.Amount + " UNT");
						sub("By",		FirstBid.Account);
						at(FirstBid);
	
					top("Last Bid", null);
						sub("Amount",	LastBid.Amount + " UNT");
						sub("By",		LastBid.Account);
						at(LastBid);
				}
			}

			if(Products != null)
			{
				top("Products", string.Join(',', Products));
			}
		}
	}
}
