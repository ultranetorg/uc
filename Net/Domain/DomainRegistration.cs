using System;
using System.IO;

namespace Uccs.Net
{
	public class DomainRegistration : Operation
	{
		public string				Name {get; set;}
		public byte					Years {get; set;}

		public bool					Exclusive => Domain.IsExclusive(Name); 
		public override string		Description => $"{Name} for {Years} years";
		public override bool		Valid => Domain.Valid(Name) && Mcv.EntityRentYearsMin <= Years && Years <= Mcv.EntityRentYearsMax;
		
		public DomainRegistration()
		{
		}

		public DomainRegistration(string domain, byte years)
		{
			if(!Net.Domain.Valid(domain))
				throw new ArgumentException("Invalid Domain name/title");

			Name = domain;
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

		public static Money CalculateFee(Time time, Money rentPerBytePerDay, int length)
		{
			var l = Math.Min(length, 10);

			return Mcv.TimeFactor(time) * rentPerBytePerDay * 1_000_000_000/(l * l * l * l);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var e = mcv.Domains.Find(Name, round.Id);
						
			if(Net.Domain.CanRegister(Name, e, round.ConsensusTime, Transaction.Signer))
			{
				var a = Affect(round, Name);

				if(a.Owner == null)
				{
					if(Exclusive) /// distribite winner bid, one time
						Reward += e.LastBid;
				}
								
				a.LastWinner	= null;
				a.LastBid		= 0;
				a.LastBidTime	= Time.Empty;
				a.FirstBidTime	= Time.Empty;
				a.Expiration	= (a.Owner != Signer ? round.ConsensusTime : a.Expiration) + Time.FromYears(Years);
				a.Owner			= Signer;
				a.SpaceReserved	= a.SpaceUsed;

				Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Net.Domain.IsExclusive(Name) ? Name.Length : (Name.Length - 1));
				Pay(round, a.SpaceUsed, Time.FromYears(Years));
			}
			else
				Error = "Cant register";
		}
	}
}
