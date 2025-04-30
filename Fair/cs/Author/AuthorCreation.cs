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

	public override void Read(BinaryReader reader)
	{
		Title = reader.ReadUtf8();
		Years = reader.ReadByte();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Years);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(Signer.AllocationSponsor != null)
		{
			Error = NotAllowedForFreeAccount;
			return;
		}

		var e = execution.Authors.Create(Signer.Address);

		Signer.Authors = [..Signer.Authors, e.Id];
		
		e.Owners	= [Signer.Id];
		e.Title		= Title;
		e.Space		= execution.Net.EntityLength;

		Prolong(execution, Signer, e, Time.FromYears(Years));
	}
}
