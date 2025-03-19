namespace Uccs.Rdn;

public class DomainRegistration : RdnOperation
{
	public string				Address {get; set;}
	public byte					Years {get; set;}
	public AccountAddress		Owner  {get; set;}
	public DomainChildPolicy	Policy {get; set;}

	public override string		Description => $"{Address} for {Years} years";
	
	public DomainRegistration ()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(!Domain.Valid(Address))
			return false;
		
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Address	= reader.ReadUtf8();
		Years = reader.ReadByte();

		if(Domain.IsChild(Address))
		{
			Owner = reader.Read<AccountAddress>();
			Policy	= reader.Read<DomainChildPolicy>();
		}
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.WriteUtf8(Address);
		writer.Write(Years);

		if(Domain.IsChild(Address))
		{
			writer.Write(Owner);
			writer.Write(Policy);
		}
	}

	public override void Execute(RdnExecution execution)
	{
		var e = execution.FindDomain(Address);

		if(Domain.IsRoot(Address))
		{
			if(!Domain.CanRegister(Address, e, execution.Time, Signer))
			{
				Error = NotAvailable;
				return;
			}

			e = execution.AffectDomain(Address);
			
			PayForName(Address, Years);
			Prolong(execution, Signer, e, Time.FromYears(Years));

			///if(Domain.IsWeb(e.Address)) /// distribite winner bid, one time
			///	Transaction.BYReturned += e.LastBid;
							
			e.Owner			= Signer.Id;
			e.LastWinner	= null;
			e.LastBid		= 0;
			e.LastBidTime	= Time.Empty;
			e.FirstBidTime	= Time.Empty;
		}
		else
		{
			if(e != null)
			{
				Error = AlreadyExists;
				return;
			}

			var o = execution.FindAccount(Owner);

			if(o == null)
			{
				Error = NotFound;
				return;
			}

			if(!RequireDomainAccess(execution, Domain.GetParent(Address), out var p))
				return;

			if(Policy < DomainChildPolicy.FullOwnership || DomainChildPolicy.FullFreedom < Policy)
			{
				Error = NotAvailable;
				return;
			}

			e = execution.AffectDomain(Address);
			
			var start = e.Expiration < execution.Time.Days ? execution.Time.Days : e.Expiration;

			e.Owner			= o.Id;
			e.ParentPolicy	= Policy;
			e.Expiration	= (short)(start + Time.FromYears(Years).Days);

			PayForName(new string(' ', Domain.NameLengthMax), Years);
		}
	}
}
