using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class CachedResource : IBinarySerializable
{
	public AutoId				Id  { get; set; }
	public Ura					Address { get; set; }
	public ResourceData			Data { get; set; }
	public DateTime				Updated;

	ResourceHub					Hub;

	public CachedResource()
	{
	}

	public CachedResource(Resource resource, Ura address)
	{
		Id = resource.Id;
		Address = address;
		Data = resource.Data;
		Updated = DateTime.UtcNow;
	}


	public override string ToString()
	{
		return Id.ToString();
	}

	public void Write(Writer writer)
	{
		writer.Write(Id);
		writer.Write(Address);
		writer.Write(Data);
	}

	public void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Address = reader.Read<Ura>();
		Data	= reader.Read<ResourceData>();

		Updated = DateTime.MinValue;
	}
}
