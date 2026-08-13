using System.Text;

namespace Uccs.Net;

public class TextToField<E> : IBinarySerializable, ITableEntry<StringId> where E : unmanaged, Enum
{
	public StringId					Id { get; set; }
	public EntityField<E>			Entity { get; set; }

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
		return $"{Id}, Entity={Entity}";
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
		Entity	= reader.Read<EntityField<E>>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Entity);
	}
}
