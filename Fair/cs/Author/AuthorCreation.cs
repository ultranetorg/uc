namespace Uccs.Fair;

public class AuthorCreation : FairOperation
{
	public string				Title { get; set; }
	public byte					Years {get; set;}

	public override string		Explanation => $"{Title}, for {Years} years";
	
	public AuthorCreation ()
	{
	}
	
	public override bool IsValid(McvNet net)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void Read(Reader reader)
	{
		Title = reader.ReadUtf8();
		Years = reader.ReadByte();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution)
	{
		if(execution.Transaction.Nonce == 0)
		{
			Error = NotAllowedForNewUser;
			return;
		}

		var e = execution.Authors.Create(User.Name);

		User.Authors = [..User.Authors, e.Id];
		
		e.Owners	= [User.Id];
		e.Title		= Title;
		e.Space		= execution.Net.EntityLength;

		execution.Prolong(User, e, Time.FromYears(Years));
		execution.PayOperationEnergy(User);
	}
}