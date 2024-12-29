namespace Uccs.Fair;

public class AuthorCreation : FairOperation
{
	public byte					Years {get; set;}

	public override string		Description => $"For {Years} years";
	
	public AuthorCreation ()
	{
	}
	
	public override bool IsValid(Mcv mcv)
	{ 
		if(Years < Mcv.EntityRentYearsMin || Years > Mcv.EntityRentYearsMax)
			return false;

		return true;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Years = reader.ReadByte();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Years);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		var e = round.AffectAuthor(Signer.Address);
		
		Signer.Authors = Signer.Authors == null ? [e.Id] : [..Signer.Authors, e.Id];
		
		e.Owner			= Signer.Id;
		e.Expiration	= round.ConsensusTime + Time.FromYears(Years);

		PayForSpacetime(e.SpaceUsed, Time.FromYears(Years));
	}
}
