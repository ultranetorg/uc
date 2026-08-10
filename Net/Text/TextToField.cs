using System.Text;

namespace Uccs.Net;

public class TextToField<E> : IBinarySerializable, ITableEntry<StringId> where E : unmanaged, Enum
{
	public StringId					Id { get; set; }
	public EntityFieldAddress<E>	Entity { get; set; }

	public bool						Deleted { get; set; }
	Mcv								Mcv;

	public TextToField()
	{
	}

	public TextToField(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {Encoding.UTF8.GetString(Id.Bytes)}, Reference={Entity}";
	}

	public object Clone()
	{
		var a = new TextToField<E> (Mcv)
				{
					Id		= Id,
					Entity	= Entity
				};
		
		return a;
	}

	public void ReadMain(Reader reader)
	{
		Read(reader);
	}

	public void WriteMain(Writer writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public void Read(Reader reader)
	{
		Entity	= reader.Read<EntityFieldAddress<E>>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Entity);
	}
}
