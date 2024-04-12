using System;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class AuthorRegistration : Operation
	{
		public string				Author {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => Net.Author.IsExclusive(Author); 
		public override string		Description => $"{Author} for {Years} years";
		public override bool		Valid => Net.Author.Valid(Author) && Mcv.EntityRentYearsMin <= Years && Years <= Mcv.EntityRentYearsMax;
		
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

		public static Money CalculateFee(Time time, Money rentPerBytePerDay, int length)
		{
			var l = Math.Min(length, 10);

			return Mcv.TimeFactor(time) * rentPerBytePerDay * 1_000_000_000/(l * l * l * l);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var e = mcv.Authors.Find(Author, round.Id);
						
			if(Net.Author.CanRegister(Author, e, round.ConsensusTime, Transaction.Signer))
			{
				if(e?.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						Fee += e.LastBid;
				}


				var a = Affect(round, Author);
				
				a.LastWinner	= null;
				a.Expiration	= (a.Owner != Signer ? round.ConsensusTime : a.Expiration) + Time.FromYears(Years);
				a.Owner			= Signer;
				a.SpaceReserved	= a.SpaceUsed;

				var f = CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Net.Author.IsExclusive(Author) ? Author.Length : (Author.Length - 1));
				Affect(round, Signer).Balance -= f;
				Fee += f;

				Pay(round, a.SpaceUsed, Time.FromYears(Years));
				//PayForEntity(round, Time.FromYears(Years));
				//PayForEntity(round, Time.FromYears(Years),	a.Resources == null ? 0 :  (a.Resources.Count(i => !i.Flags.HasFlag(ResourceFlags.Sealed)) + 
				//																		a.Resources.Sum(r => r.Outbounds.Count(i => !i.Flags.HasFlag(ResourceLinkFlag.Sealed)))));
				//PayForBytes(round, a.SpaceUsed, Time.FromYears(Years));
			}
			else
				Error = "Failed";
		}
	}
}
