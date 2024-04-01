using System;
using System.IO;

namespace Uccs.Net
{
	public class AuthorRegistration : Operation
	{
		public string				Author {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => AuthorEntry.IsExclusive(Author); 
		public override string		Description => $"{Author} for {Years} years";
		public override bool		Valid => Net.Author.Valid(Author) && Mcv.EntityAllocationYearsMin <= Years && Years <= Mcv.EntityAllocationYearsMax;
		
		//public static Coin			GetCost(Coin factor, int years) => Chainbase.AuthorFeePerYear * years * (Emission.FactorEnd - factor) / Emission.FactorEnd;

		public AuthorRegistration()
		{
		}

		public AuthorRegistration(string author, byte years)
		{
			if(!Uccs.Net.Author.Valid(author))
				throw new ArgumentException("Invalid Author name/title");

			Author = author;
			Years = years;
		}

		public override void ReadConfirmed(BinaryReader r)
		{
			Author	= r.ReadUtf8();
			Years	= r.ReadByte();
		}

		public override void WriteConfirmed(BinaryWriter w)
		{
			w.WriteUtf8(Author);
			w.Write(Years);
		}

		public override void Execute(Mcv chain, Round round)
		{
			var e = chain.Authors.Find(Author, round.Id);
						
			if(Net.Author.CanRegister(Author, e, round.ConsensusTime, Transaction.Signer))
			{
				if(e?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						Fee += e.LastBid;
				}


				var a = Affect(round, Author);
				
				a.LastWinner	= null;
				a.Owner			= Signer;
				a.Expiration	= round.ConsensusTime + Time.FromYears(Years);

				PayForEntity(round, Time.FromYears(Years));
				PayForEntity(round, Time.FromYears(Years), a.Resources.Length);
				PayForBytes(round, a.SpaceUsed, Time.FromYears(Years));
			}
			else
				Error = "Failed";
		}
	}
}
