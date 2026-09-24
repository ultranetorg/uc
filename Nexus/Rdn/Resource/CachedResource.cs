using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class CachedResource
{
	public AutoId				Id  { get; set; }
	//public Ura					Address { get; set; }
	public ResourceData			Data { get; set; }
	public DateTime				Updated { get; set; }

	ResourceHub					Hub;

	public CachedResource()
	{
	}

	public CachedResource(Resource resource)
	{
		Id = resource.Id;
		Data = resource.Data;

		Updated = DateTime.UtcNow;
	}

	public CachedResource(AutoId id, ResourceData data)
	{
		Id = id;
		Data = data;
	}

	public override string ToString()
	{
		return Id.ToString();
	}
}
