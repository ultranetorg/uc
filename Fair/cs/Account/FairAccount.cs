namespace Uccs.Fair;

public class FairAccountEntry : AccountEntry
{
	public EntityId[]		Authors;
	public EntityId[]		Sites;
	public EntityId[]		Reviews;
	public int				Approvals;
	public int				Rejections;
	public EntityId			AllocationSponsor;
	public byte				AllocationSponsorClass;

	public FairAccountEntry(Mcv mcv) : base(mcv)
	{
	}

	public override AccountEntry Clone()
	{
		var a = base.Clone() as FairAccountEntry;

		a.Authors				 = Authors;
		a.Sites					 = Sites;
		a.Reviews				 = Reviews;
		a.Approvals				 = Approvals;
		a.Rejections			 = Rejections;
		a.AllocationSponsor		 = AllocationSponsor;
		a.AllocationSponsorClass = AllocationSponsorClass;

		return a;
	}

	public override void WriteMain(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(Authors);
		writer.Write(Sites);
		writer.Write(Reviews);

		writer.WriteNullable(AllocationSponsor);
		
		if(AllocationSponsor != null)
		{
			writer.Write(AllocationSponsorClass);
			writer.Write7BitEncodedInt(Approvals);
			writer.Write7BitEncodedInt(Rejections);
		}
	}

	public override void ReadMain(BinaryReader reader)
	{
		base.Read(reader);

		Authors					= reader.ReadArray<EntityId>();
		Sites					= reader.ReadArray<EntityId>();
		Reviews					= reader.ReadArray<EntityId>();

		AllocationSponsor		= reader.ReadNullable<EntityId>();
		
		if(AllocationSponsor != null)
		{
			AllocationSponsorClass	= reader.ReadByte();
			Approvals				= reader.Read7BitEncodedInt();
			Rejections				= reader.Read7BitEncodedInt();
		}
	}
}
