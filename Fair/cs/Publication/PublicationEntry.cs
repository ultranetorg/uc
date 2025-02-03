namespace Uccs.Fair;

public class PublicationEntry : Publication, ITableEntry
{
	public BaseId		BaseId => Id;
	public bool			Deleted { get; set; }
	FairMcv				Mcv;

	public PublicationEntry()
	{
	}

	public PublicationEntry(FairMcv mcv)
	{
		Mcv = mcv;
	}

	public PublicationEntry Clone()
	{
		return new(Mcv){Id			= Id,
						Category	= Category,
						Creator		= Creator,
						Product		= Product,
						Status		= Status,
						Sections	= Sections,
						Reviews	= Reviews};
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

