namespace Uccs.Smp;

public class SmpAccountEntry : AccountEntry
{
	public EntityId[]		Authors;
	public EntityId[]		Sites;

	public SmpAccountEntry(Mcv mcv) : base(mcv)
	{
	}

	public override AccountEntry Clone()
	{
		var a = base.Clone() as SmpAccountEntry;

		a.Authors = Authors?.ToArray();
		a.Sites = Sites?.ToArray();

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
