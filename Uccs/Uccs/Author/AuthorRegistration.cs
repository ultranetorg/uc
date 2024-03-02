using System;
using System.IO;

namespace Uccs.Net
{
	public class AuthorRegistration : Operation
	{
		public string				Name {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => AuthorEntry.IsExclusive(Name); 
		public override string		Description => $"{Name} for {Years} years";
		public override bool		Valid => Author.Valid(Name) && Mcv.EntityAllocationYearsMin <= Years && Years <= Mcv.EntityAllocationYearsMax;
		
		//public static Coin			GetCost(Coin factor, int years) => Chainbase.AuthorFeePerYear * years * (Emission.FactorEnd - factor) / Emission.FactorEnd;

		public AuthorRegistration()
		{
		}

		public AuthorRegistration(string author, byte years)
		{
			if(!Author.Valid(author))
				throw new ArgumentException("Invalid Author name/title");

			Name = author;
			Years = years;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			Name	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Name);
			w.Write(Years);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var e = chain.Authors.Find(Name, round.Id);
						
			if(Author.CanRegister(Name, e, round.ConsensusTime, Transaction.Signer))
			{
				if(e?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						Fee += e.LastBid;
				}


				var a = Affect(round, Name);
				
				a.LastWinner	= null;
				a.Owner			= Signer;
				a.Expiration	= round.ConsensusTime + Time.FromYears(Years);

				PayForEntity(round, Time.FromYears(Years));
				PayForBytes(round, a.SpaceUsed, Time.FromYears(Years));

				foreach(var i in a.Resources)
					PayForEntity(round, Time.FromYears(Years));
			}
			else
				Error = "Failed";
		}
	}
}
