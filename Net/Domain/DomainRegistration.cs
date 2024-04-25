using System;
using System.IO;

namespace Uccs.Net
{
	public class DomainRegistration : Operation
	{
		public string				Address {get; set;}
		public byte					Years {get; set;}
		public AccountAddress		Owner  {get; set;}
		public DomainChildPolicy	ChildPolicy  {get; set;}

		public bool					Exclusive => Domain.IsWeb(Address); 
		public override string		Description => $"{Address} for {Years} years";
		public override bool		Valid => Domain.Valid(Address) && Mcv.EntityRentYearsMin <= Years && Years <= Mcv.EntityRentYearsMax;
		
		public DomainRegistration()
		{
		}

		//public DomainRegistration(string domain, byte years, AccountAddress owner, DomainChildPolicy childpolicy)
		//{
		//	if(!Domain.Valid(domain))
		//		throw new ArgumentException("Invalid Domain name/title");
		//
		//	Address = domain;
		//	Years = years;
		//	Owner = owner;
		//	ChildPolicy = childpolicy;
		//}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Address		= reader.ReadUtf8();
			Years		= reader.ReadByte();
			ChildPolicy	= (DomainChildPolicy)reader.ReadByte();

			if(ChildPolicy != DomainChildPolicy.None)
			{
				Owner = reader.Read<AccountAddress>();
			}
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Address);
			writer.Write(Years);
			writer.Write((byte)ChildPolicy);

			if(ChildPolicy != DomainChildPolicy.None)
			{
				writer.Write(Owner);
			}
		}

		public static Money CalculateFee(Time time, Money rentPerBytePerDay, int length)
		{
			var l = Math.Min(length, 10);

			return Mcv.TimeFactor(time) * rentPerBytePerDay * 1_000_000_000/(l * l * l * l);
		}

		public override void Execute(Mcv mcv, Round round)
		{
			var e = mcv.Domains.Find(Address, round.Id);
						
			if(Domain.IsRoot(Address))
			{
				if(Domain.CanRegister(Address, e, round.ConsensusTime, Transaction.Signer))
				{
					var a = Affect(round, Address);
									
					a.Expiration	= (a.Owner != Signer ? round.ConsensusTime : a.Expiration) + Time.FromYears(Years);
					a.SpaceReserved	= a.SpaceUsed;
	
					if(a.Owner == null)
					{
						if(Exclusive) /// distribite winner bid, one time
							Reward += e.LastBid;
						
						a.Owner			= Signer;
						a.LastWinner	= null;
						a.LastBid		= 0;
						a.LastBidTime	= Time.Empty;
						a.FirstBidTime	= Time.Empty;
					}
	
					Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.IsWeb(Address) ? Address.Length : (Address.Length - Domain.NormalPrefix.ToString().Length));
					Pay(round, a.SpaceUsed, Time.FromYears(Years));
				}
				else
					Error = "Cant register";
			} 
			else
			{
				if(e == null)
				{
					if(RequireDomain(round, Signer, Domain.GetParent(Address), out var p) == false)
					{
						return;
					}

					var a = Affect(round, Address);

					if(a.ParentPolicy == DomainChildPolicy.FullFreedom)
					{
						Error = NotPermitted;
						return;
					}

					if(ChildPolicy < DomainChildPolicy.FullOwnership || ChildPolicy > DomainChildPolicy.FullFreedom)
					{
						Error = NotPermitted;
						return;
					}

					a.Expiration	= round.ConsensusTime + Time.FromYears(Years);
					a.Owner			= Owner;
					a.ParentPolicy	= ChildPolicy;

					Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.NameLengthMax);
				} 
				else
				{
					if(Domain.CanRegister(Address, e, round.ConsensusTime, Transaction.Signer))
					{
						var a = Affect(round, Address);

						a.Expiration	= a.Expiration + Time.FromYears(Years);
						a.SpaceReserved	= a.SpaceUsed;
	
						Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.NameLengthMax);
						Pay(round, a.SpaceUsed, Time.FromYears(Years));
					}
					else
						Error = "Cant register";
				}
			}
		}
	}
}
