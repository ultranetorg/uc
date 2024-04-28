using System;
using System.IO;

namespace Uccs.Net
{
	public enum DomainAction
	{
		None, Acquire, Renew, CreateSubdomain, Transfer, ChangePolicy
	}

	public class DomainUpdation : Operation
	{
		public string				Address {get; set;}
		public DomainAction			Action  {get; set;}
		public byte					Years {get; set;}
		public AccountAddress		Owner  {get; set;}
		public DomainChildPolicy	Policy {get; set;}

		public bool					Exclusive => Domain.IsWeb(Address); 
		public override string		Description => $"{Address} for {Years} years";
		
		public DomainUpdation()
		{
		}
		
		public override bool IsValid(Mcv mcv)
		{ 
			if(!Domain.Valid(Address))
				return false;
			
			if(	(Action == DomainAction.Acquire || Action == DomainAction.Renew || Action == DomainAction.CreateSubdomain) && 
				(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
				return false;

			if((Action == DomainAction.CreateSubdomain || Action == DomainAction.ChangePolicy) && Policy == DomainChildPolicy.None)
				return false;

			return true;
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			Address	= reader.ReadUtf8();
			Action	= (DomainAction)reader.ReadByte();

			if(Action == DomainAction.Acquire || Action == DomainAction.Renew || Action == DomainAction.CreateSubdomain)
				Years = reader.ReadByte();

			if(Action == DomainAction.CreateSubdomain || Action == DomainAction.Transfer)
				Owner = reader.Read<AccountAddress>();
			
			if(Action == DomainAction.CreateSubdomain || Action == DomainAction.ChangePolicy)
				Policy	= (DomainChildPolicy)reader.ReadByte();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			writer.WriteUtf8(Address);
			writer.Write((byte)Action);

			if(Action == DomainAction.Acquire || Action == DomainAction.Renew || Action == DomainAction.CreateSubdomain)
				writer.Write(Years);

			if(Action == DomainAction.CreateSubdomain || Action == DomainAction.Transfer)
				writer.Write(Owner);

			if(Action == DomainAction.CreateSubdomain || Action == DomainAction.ChangePolicy)
				writer.Write((byte)Policy);

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
				if(	Action == DomainAction.Acquire  ||
					Action == DomainAction.Renew)
				{	
					if(!Domain.CanRegister(Address, e, round.ConsensusTime, Signer))
					{
						Error = NotAvailable;
						return;
					}

					e = Affect(round, Address);
					e.SpaceReserved	= e.SpaceUsed;
		
					if(Action == DomainAction.Acquire)
					{
						if(Exclusive) /// distribite winner bid, one time
							Reward += e.LastBid;
								
						e.Expiration	= round.ConsensusTime + Time.FromYears(Years);
						e.Owner			= Signer;
						e.LastWinner	= null;
						e.LastBid		= 0;
						e.LastBidTime	= Time.Empty;
						e.FirstBidTime	= Time.Empty;
					}

					if(Action == DomainAction.Renew)
					{
						e.Expiration = e.Expiration + Time.FromYears(Years);
					}
							
					Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.IsWeb(Address) ? Address.Length : (Address.Length - Domain.NormalPrefix.ToString().Length));
					Pay(round, e.SpaceUsed, Time.FromYears(Years));
				}

				if(Action == DomainAction.Transfer)
				{
					if(!Domain.IsOwner(e, Signer, round.ConsensusTime))
					{
						Error = NotOwner;
						return;
					}

					var a = Affect(round, Address);
					a.Owner	= Owner;
				}
			} 
			else
			{
				var p = mcv.Domains.Find(Domain.GetParent(Address), round.Id);

				if(p == null)
				{
					Error = NotFound;
					return;
				}

				if(Action == DomainAction.CreateSubdomain)
				{
					if(e != null)
					{
						Error = AlreadyExists;
						return;
					}

					if(!Domain.IsOwner(p, Signer, round.ConsensusTime))
					{
						Error = NotOwner;
						return;
					}

					if(Policy < DomainChildPolicy.FullOwnership || DomainChildPolicy.FullFreedom < Policy)
					{
						Error = NotAvailable;
						return;
					}

					e = Affect(round, Address);

					e.Owner			= Owner;
					e.ParentPolicy	= Policy;
					e.Expiration	= round.ConsensusTime + Time.FromYears(Years);

					Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.NameLengthMax);
				}

				if(Action == DomainAction.Renew)
				{
					if(!Domain.CanRenew(e, Signer, round.ConsensusTime))
					{
						Error = NotAvailable;
						return;
					}

					e = Affect(round, Address);

					e.Expiration	= e.Expiration + Time.FromYears(Years);
					e.SpaceReserved	= e.SpaceUsed;
	
					Affect(round, Signer).Balance -= CalculateFee(Time.FromYears(Years), round.RentPerBytePerDay, Domain.NameLengthMax);
					Pay(round, e.SpaceUsed, Time.FromYears(Years));
				}

				if(Action == DomainAction.ChangePolicy)
				{
					if(e == null)
					{
						Error = NotFound;
						return;
					}

					if(!Domain.IsOwner(p, Signer, round.ConsensusTime))
					{
						Error = NotOwner;
						return;
					}

					if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsExpired(e, round.ConsensusTime))
					{
						Error = NotAvailable;
						return;
					}

					e = Affect(round, Address);
					e.ParentPolicy = Policy;
				}

				if(Action == DomainAction.Transfer)
				{
					if(e == null)
					{
						Error = NotFound;
						return;
					}

					if(e.ParentPolicy == DomainChildPolicy.FullOwnership && !Domain.IsOwner(p, Signer, round.ConsensusTime))
					{
						Error = NotAvailable;
						return;
					}

					if(e.ParentPolicy == DomainChildPolicy.FullFreedom && !Domain.IsOwner(e, Signer, round.ConsensusTime) && 
																		  !(Domain.IsOwner(p, Signer, round.ConsensusTime) && Domain.IsExpired(e, round.ConsensusTime)))
					{
						Error = NotAvailable;
						return;
					}

					e = Affect(round, Address);
					e.Owner	= Owner;
				}
			}
		}
	}
}
