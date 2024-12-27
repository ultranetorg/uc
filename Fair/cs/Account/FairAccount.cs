namespace Uccs.Fair;

public class FairAccountEntry : AccountEntry
{
	public EntityId[]		Authors;
	public EntityId[]		Catalogues;

	public FairAccountEntry(Mcv mcv) : base(mcv)
	{
	}

	public override AccountEntry Clone()
	{
		var a = base.Clone() as FairAccountEntry;

		a.Authors = Authors?.ToArray();
		a.Catalogues = Catalogues?.ToArray();

		return a;
	}

	public override void WriteMain(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(Authors);
		writer.Write(Catalogues);
	}

	public override void ReadMain(BinaryReader reader)
	{
		base.Read(reader);

		Authors = reader.ReadArray<EntityId>();
		Catalogues = reader.ReadArray<EntityId>();
	}
}
