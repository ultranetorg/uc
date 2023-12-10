using System;
using System.IO;

namespace Uccs.Net
{
	public class AuthorRegistration : Operation
	{
		public string				Name {get; set;}
		public string				Title {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => AuthorEntry.IsExclusive(Name); 
		public override string		Description => $"{Name} ({Title}) for {Years} years";
		public override bool		Valid => Author.Valid(Name) && Mcv.EntityAllocationYearsMin <= Years && Years <= Mcv.EntityAllocationYearsMax;
		
		//public static Coin			GetCost(Coin factor, int years) => Chainbase.AuthorFeePerYear * years * (Emission.FactorEnd - factor) / Emission.FactorEnd;

		public AuthorRegistration()
		{
		}

		public AuthorRegistration(string author, string title, byte years)
		{
			if(!Author.Valid(author))
				throw new ArgumentException("Invalid Author name/title");

			Name = author;
			Title = title;
			Years = years;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			Title	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.WriteUtf8(Title);
			w.Write(Years);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var e = chain.Authors.Find(Name, round.Id);
						
			if(Author.CanRegister(Name, e, round.ConfirmedTime, Transaction.Signer))
			{
				if(e?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						round.Fees += e.LastBid;
				}


				var a = Affect(round, Name);
				
				a.LastWinner	= null;
				a.Title			= Title;
				a.Owner			= Signer;
				a.Expiration	= round.ConfirmedTime + Time.FromYears(Years);

				Pay(round, Mcv.EntityAllocation, Years);
			}
			else
				Error = "Failed";
		}
	}
}
