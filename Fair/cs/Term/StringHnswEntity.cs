namespace Uccs.Fair;

public class StringHnswEntity : HnswNode<string>
{
	public string								Text { get; set; }
	public override string						Data => Text;
	public SortedDictionary<AutoId, AutoId>		References { get; set; }

	public StringHnswEntity Clone()
	{
		var a = new StringHnswEntity() {Id			= Id,
										Connections	= Connections,
										Text		= Text,
										References	= References};

		return a;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, References={References.Count}";
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		Text		= reader.ReadUtf8();
		References	= reader.ReadSortedDictionary(reader.Read<AutoId>, reader.Read<AutoId>);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.WriteUtf8(Text);
		writer.Write(References, i => writer.Write(i), i => writer.Write(i));
	}

	public override void ReadMain(BinaryReader r)
	{
		Read(r);
	}

	public override void WriteMain(BinaryWriter w)
	{
		Write(w);
	}

	public override void Cleanup(Round lastInCommit)
	{
	}
}
