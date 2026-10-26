namespace Uccs.Rdn;

public class Release : ITableEntry<Urn>, IBinarySerializable
{
	public Urn					Id { get; set; }
	public int					Refs { get; set; }

	public bool					Deleted { get; set; }
	RdnMcv						Mcv;

	public override string ToString()
	{
		return $"{Id}, {nameof(Refs)}={Refs}";
	}

	public Release()
	{
	}

	public Release(RdnMcv rdn)
	{
		Mcv = rdn;
	}

	public object Clone()
	{
		return	new Release(Mcv)
				{
					Id = Id,
					Refs = Refs,
				};
	}

	public void WriteMain(Writer writer)
	{
		writer.Write7BitEncodedInt(Refs);
	}

	public void ReadMain(Reader reader)
	{
		Refs = reader.Read7BitEncodedInt();;
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		WriteMain(writer);
	}

	public void Read(Reader reader)
	{
		Id = reader.Read<Urn>();
		ReadMain(reader);
	}

	public void Cleanup(Round lastInCommit)
	{
	}
}
