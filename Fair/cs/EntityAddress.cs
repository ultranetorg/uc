namespace Uccs.Fair;

public class EntityAddress : IBinarySerializable
{
	public AutoId		Id { get; set; }
	public FairTable	Table { get; set; }

	public EntityAddress(AutoId id, FairTable table)
	{
		Id = id;
		Table = table;
	}

	public EntityAddress()
	{
	}

	public override string ToString()
	{
		return $"{Table}/{Id}";
	}

	public void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Table	= reader.Read<FairTable>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Table);
	}
}