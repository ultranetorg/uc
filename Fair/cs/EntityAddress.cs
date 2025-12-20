namespace Uccs.Fair;

public class EntityAddress : IBinarySerializable
{
	public AutoId		Id { get; set; }
	public FairTable	Table { get; set; }

	public EntityAddress(FairTable table, AutoId id)
	{
		Table = table;
		Id = id;
	}

	public EntityAddress()
	{
	}

	public override string ToString()
	{
		return $"{Table}/{Id}";
	}

	public static EntityAddress Parse(string text)
	{
		var i = text.IndexOf('/');
		return new EntityAddress(Enum.Parse<FairTable>(text.Substring(0, i)), AutoId.Parse(text.Substring(i + 1)));
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