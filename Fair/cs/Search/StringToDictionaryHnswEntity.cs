namespace Uccs.Fair;

public abstract class StringHnswEntity : HnswNode<string>
{
	public string					Text { get; set; }
	public override string			Data => Text;

	public void Copy(StringHnswEntity entity)
	{
		entity.Id			= Id;
		entity.Connections	= Connections;
		entity.Text			= Text;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		Text = reader.ReadUtf8();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.WriteUtf8(Text);
	}
}

public class StringToOneHnswEntity : StringHnswEntity
{
	public SortedSet<AutoId>		References { get; set; }

	public override object Clone()
	{
		var a = new StringToOneHnswEntity {References = References};
		
		Copy(a);

		return a;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, References={References.Count}";
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		References	= reader.ReadSortedSet(reader.Read<AutoId>);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write(References);
	}
}

public class StringToDictionaryHnswEntity : StringHnswEntity
{
	public SortedDictionary<AutoId, AutoId>		References { get; set; }

	public override object Clone()
	{
		var a = new StringToDictionaryHnswEntity {References = References};
		
		Copy(a);

		return a;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, References={References.Count}";
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);
		References	= reader.ReadSortedDictionary(reader.Read<AutoId>, reader.Read<AutoId>);
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);
		writer.Write(References, i => writer.Write(i), i => writer.Write(i));
	}
}
