namespace Uccs.Fair;

public class FairAccountEntry : AccountEntry
{
	public EntityId[]		Authors;
	public EntityId[]		Sites;

	public FairAccountEntry(Mcv mcv) : base(mcv)
	{
	}

	public override AccountEntry Clone()
	{
		var a = base.Clone() as FairAccountEntry;

		a.Authors = Authors;
		a.Sites = Sites;

		return a;
	}

	public override void WriteMain(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(Authors);
		writer.Write(Sites);
	}

	public override void ReadMain(BinaryReader reader)
	{
		base.Read(reader);

		Authors = reader.ReadArray<EntityId>();
		Sites = reader.ReadArray<EntityId>();
	}
}
