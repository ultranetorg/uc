namespace Uccs.Smp;

public class ReviewEntry : Review, ITableEntry
{
	public BaseId		BaseId => Id;
	public bool			Deleted { get; set; }
	SmpMcv				Mcv;

	public ReviewEntry()
	{
	}

	public ReviewEntry(SmpMcv mcv)
	{
		Mcv = mcv;
	}

	public ReviewEntry Clone()
	{
		return new(Mcv){Id			= Id,
						Publication = Publication,
						User		= User,
						Status		= Status,
						Text		= Text,
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

