using System;
using System.IO;

namespace Uccs.Net
{
	public class AuthorRegistration : Operation
	{
		
		public string				Name;
		public string				Title {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => AuthorEntry.IsExclusive(Name); 
		public override string		Description => $"{Name} ({Title}) for {Years} years";
		public override bool		Valid => IsValid(Name, Title) && 0 < Years;
		
		//public static Coin			GetCost(Coin factor, int years) => Chainbase.AuthorFeePerYear * years * (Emission.FactorEnd - factor) / Emission.FactorEnd;

		public AuthorRegistration()
		{
		}

		public AuthorRegistration(AccountKey signer, string author, string title, byte years)
		{
			if(!Operation.IsValid(author, title))
				throw new ArgumentException("Invalid Author name/title");

			Signer = signer;
			Name = author;
			Title = title;
			Years = years;
		}

		protected override void ReadConfirmed(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			Title	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		protected override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.WriteUtf8(Title);
			w.Write(Years);
		}

		public override void Execute(Chainbase chain, Round round)
		{
			var a = chain.Authors.Find(Name, round.Id);
						
			if(Author.CanRegister(Name, a, round.ConfirmedTime, Signer))
			{
				if(a?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						round.Fees += a.LastBid;
				}


				a = round.AffectAuthor(Name);
				
				a.LastWinner	= null;
				a.Title			= Title;
				a.Owner			= Signer;
				a.Expiration	= round.ConfirmedTime + ChainTime.FromYears(Years);

				var cost = CalculateSpaceFee(round.Factor, CalculateSize(), Years);
				
				round.AffectAccount(Signer).Balance -= cost;
				round.Fees += cost;
			}
			else
				Error = "Failed";
		}
	}
}
