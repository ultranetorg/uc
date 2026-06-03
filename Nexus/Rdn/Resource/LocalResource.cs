using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class LocalResource
{
	public AutoId				Id  { get; set; }
	public Ura					Address { get; set; }
	public bool					Resolving { get; set; }
	public ResourceData			Data { get; set; }

	ResourceHub					Hub;

	public LocalResource()
	{
	}

	public LocalResource(ResourceHub hub, Ura resource)
	{
		Hub = hub;
		Address = resource;
	}

	public void AddData(ResourceData data)
	{
		Data = data;
		Save();
	}

	public void AddData(DataType type, object value)
	{
		var v = ResourceData.Serialize(value);

		Data =  new ResourceData(type, v);
	
		Save();
	}

	internal void Load()
	{
		var d = Hub.Node.Database.Get(Encoding.UTF8.GetBytes(Address.ToString()), Hub.ResourceFamily);
									
		if(d != null)
		{
			var r = new Reader(d);

			Data = r.ReadNullable<ResourceData>();
		}
	}

	internal void Save()
	{
		using(var b = new WriteBatch())
		{
			var s = new MemoryStream();
			var w = new Writer(s);
			
			w.WriteNullable(Data);

			b.Put(Encoding.UTF8.GetBytes(Address.ToString()), s.ToArray(), Hub.ResourceFamily);
			
			Hub.Node.Database.Write(b);
		}
	}

	public override string ToString()
	{
		return Address.ToString();
	}
}
