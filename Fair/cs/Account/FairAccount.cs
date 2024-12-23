namespace Uccs.Fair;

public class FairAccountEntry : AccountEntry
{
	public EntityId[]		Authors;

	public FairAccountEntry(Mcv mcv) : base(mcv)
	{
	}

	public override AccountEntry Clone()
	{
		var a = base.Clone() as FairAccountEntry;

		a.Authors = Authors?.ToArray();

		return a;
	}

	public override void WriteMain(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(Authors);
	}

	public override void ReadMain(BinaryReader reader)
	{
		base.Read(reader);

		Authors = reader.ReadArray<EntityId>();
	}
}
