namespace Uccs.Net;

public class EntityAddress : IBinarySerializable
{
	public AutoId			Id { get; set; }
	public byte				Table { get; set; }

	public bool				IsValid(McvNet net) => Table < net.TablesCount;
	public static string	ToString<T>(T t, AutoId id) where T : unmanaged, Enum => $"{t}/{id}";

	public EntityAddress(byte table, AutoId id)
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

	public static EntityAddress Parse<T>(string text) where T : unmanaged, Enum
	{
		var i = text.IndexOf('/');
		return new EntityAddress((byte)(object)Enum.Parse<T>(text.AsSpan(0, i)), AutoId.Parse(text.AsSpan(i + 1)));
	}

	public static bool TryParse<T>(string text, out EntityAddress address) where T : unmanaged, Enum
	{
		address = null;

		var i = text.IndexOf('/');

		if(i == -1 || i == 0 || i == text.Length-1)
			return false;

		if(!Enum.TryParse<T>(text.AsSpan(0, i), out var t))
			return false;

		if(!AutoId.TryParse(text.Substring(i + 1), out var id))
			return false;

		address = new EntityAddress((byte)(object)t, id);

		return true;
	}

	public void Read(BinaryReader reader)
	{
		Id		= reader.Read<AutoId>();
		Table	= reader.ReadByte();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Id);
		writer.Write(Table);
	}
}