namespace Uccs.Fair;

class StoreRenewal : FairOperation
{
	public AutoId				StoreId { get; set; }
	public byte					Years { get; set; }

	public override string		Explanation => $"{StoreId}, {Years}";
	
	public StoreRenewal()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if((Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax))
			return false;

		return true;
	}

	public override void Read(Reader reader)
	{
		StoreId	= reader.Read<AutoId>();
		Years	= reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.Write(StoreId);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(!IsModerator(execution, StoreId, out var s, out Error))
			return;
		
		s = execution.Stores.Affect(StoreId);

		if(!(s as IExpirable).CanRenew(execution.Time, Time.FromYears(Years)))
		{
			Error = NotAvailable;
			return;
		}

		execution.Prolong(s, s, Time.FromYears(Years));
	}
}
