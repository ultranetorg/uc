using System.Collections.Immutable;
using System.Text;

namespace Uccs.Net;

public class TextToFields<E> : IBinarySerializable, ITableEntry<RawId> where E : unmanaged, Enum
{
	public RawId								Id { get; set; }
	public ImmutableList<EntityFieldAddress<E>>	Entities { get; set; }

	public bool									Deleted { get; set; }
	Mcv											Mcv;

	public TextToFields()
	{
	}

	public TextToFields(Mcv mcv)
	{
		Mcv = mcv;
	}

	public override string ToString()
	{
		return $"{Id}, {Encoding.UTF8.GetString(Id.Bytes)}, {nameof(Entities)}={Entities}";
	}

	public static RawId	GetId(string t)
	{
		var b = Encoding.UTF8.GetBytes(t);

		return new RawId(b);
	}

	public object Clone()
	{
		var a = new TextToFields<E> (Mcv)
				{
					Id			= Id,
					Entities	= Entities
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
		Entities = reader.ReadImmutableList<EntityFieldAddress<E>>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Entities);
	}
}
