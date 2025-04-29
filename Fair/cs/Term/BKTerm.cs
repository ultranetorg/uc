using System.Collections.Generic;
using System.Text;

namespace Uccs.Fair;

public abstract class BKTerm : IBinarySerializable, ITableEntry
{
	public RawId							Id { get; set; }
	public string							Word => _Text ??= Encoding.UTF8.GetString(Id.Bytes);
	public SortedDictionary<byte, RawId>	Children { get; set; }

	public EntityId							Key => Id;
	public bool								Deleted { get; set; }
	protected FairMcv						Mcv;
	string									_Text;

	public abstract BKTerm					Clone();

	public BKTerm()
	{
	}

	public BKTerm(FairMcv mcv)
	{
		Mcv = mcv;
	}
	
	public static RawId	GetId(string t)
	{
		var b = Encoding.UTF8.GetBytes(t);

		return new RawId(b);
	}


	public void ReadMain(BinaryReader reader)
	{
		Read(reader);
	}

	public void WriteMain(BinaryWriter writer)
	{
		Write(writer);
	}

	public void Cleanup(Round lastInCommit)
	{
	}

	public virtual void Read(BinaryReader reader)
	{
		Id			= reader.Read<RawId>();
		Children	= reader.ReadSortedDictionary(() => reader.ReadByte(), () => reader.Read<RawId>());
	}

	public virtual void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Children, i => { writer.Write(i.Key); writer.Write(i.Value); });
	}
}

public class EntityTerm : BKTerm
{
	public AutoId[]	References { get; set; }

	public EntityTerm()
	{
	}

	public EntityTerm(FairMcv mcv) : base(mcv)
	{
	}

	public override EntityTerm Clone()
	{
		var a = new EntityTerm(Mcv){Id			= Id,
									Children	= Children,
									References	= References};

		return a;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		References = reader.ReadArray<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(References);
	}
}

public class SiteTerm : BKTerm
{
	public SortedDictionary<AutoId, AutoId[]>	References { get; set; }

	public SiteTerm()
	{
	}

	public SiteTerm(FairMcv mcv) : base(mcv)
	{
	}

	public override SiteTerm Clone()
	{
		var a = new SiteTerm(Mcv)  {Id			= Id,
									Children	= Children,
									References	= References};

		return a;
	}

	public override void Read(BinaryReader reader)
	{
		base.Read(reader);

		References = reader.ReadSortedDictionary(() => reader.Read<AutoId>(), () => reader.ReadArray<AutoId>());
	}

	public override void Write(BinaryWriter writer)
	{
		base.Write(writer);

		writer.Write(References, i => { writer.Write(i.Key); writer.Write(i.Value); });
	}
}