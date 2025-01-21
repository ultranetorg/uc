namespace Uccs.Smp;

public class CategoryEntry : Category, ITableEntry
{
	public BaseId		BaseId => Id;
	public bool			Deleted { get; set; }
	SmpMcv				Mcv;
	bool				PublicationAffected;

	public CategoryEntry()
	{
	}

	public CategoryEntry(SmpMcv mcv)
	{
		Mcv = mcv;
	}

	public CategoryEntry Clone()
	{
		return new(Mcv){Id			 = Id,
						Site		 = Site,
						Parent		 = Parent,
						Title		 = Title,
						Categories	 = Categories,
						Publications = Publications
						};
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

 	//public void AffectPublications()
 	//{
 	//	if(!PublicationAffected)
 	//	{
 	//		Publications = [..Publications];
 	//		PublicationAffected = true;
 	//	}
 	//}
}

