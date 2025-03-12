namespace Uccs.Fair;

public class ReviewEntry : Review, ITableEntry
{
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public ReviewEntry()
	{
	}

	public ReviewEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public ReviewEntry Clone()
	{
		return new(Mcv){Id			= Id,
						Publication = Publication,
						Creator		= Creator,
						Status		= Status,
						Rate		= Rate,
						Text		= Text,
						TextNew		= TextNew,
						Rating		= Rating,
						Created		= Created};
	}
		

	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void WriteMore(BinaryWriter w)
	{
	}

	public void ReadMore(BinaryReader r)
	{
	}

	public void Cleanup(Round lastInCommit)
	{
	}

// 	public void AffectSecurity()
// 	{
// 		if(!SecurityAffected)
// 		{
// 			Security = Security?.Clone() ?? new () {Permissions = []};
// 			SecurityAffected = true;
// 		}
// 	}
}

