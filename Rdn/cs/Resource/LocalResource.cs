using System.Text;
using RocksDbSharp;

namespace Uccs.Rdn;

public class LocalResource
{
	public EntityId				Id  { get; set; }
	public Ura					Address { get; set; }
	public List<ResourceData>	Datas { get; set; }

	public ResourceData			Last => Datas.LastOrDefault();

	ResourceHub					Hub;

	public LocalResource()
	{
	}

	public LocalResource(ResourceHub hub, Ura resource)
	{
		Hub = hub;
		Address = resource;
	}

	//public T LastAs<T>() where T : IBinarySerializable, new()
	//{
	//	//var t = new T();
	//	//t.Read(new BinaryReader(new MemoryStream(Last)));
	//	//return t;
	//	return Last.Read<T>();
	//}

	public void AddData(ResourceData data)
	{
		if(Datas == null)
			Datas = new();

		var i = Datas.Find(i => i.Equals(data));

		if(i != null)
		{
			Datas.Remove(i);
			Datas.Add(i);
		}
		else
		{
			Datas.Add(data);
		}
		
		Save();
	}

	public void AddData(DataType type, object value)
	{
		if(Datas == null)
			Datas = new();

		var v = ResourceData.Serialize(value);
		var i = Datas.Find(i => i.Type == type && i.Value.SequenceEqual(v));

		if(i != null)
		{
			Datas.Remove(i);
			Datas.Add(i);
		}
		else
		{ 
			Datas.Add(new ResourceData(type, v));
		}
	
		Save();
	}

	internal void Load()
	{
		var d = Hub.Node.Database.Get(Encoding.UTF8.GetBytes(Address.ToString()), Hub.ResourceFamily);
									
		if(d != null)
		{
			var s = new MemoryStream(d);
			var r = new BinaryReader(s);

			Datas = r.ReadList<ResourceData>();
		}
	}

	internal void Save()
	{
		using(var b = new WriteBatch())
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);
			
			w.Write(Datas);

			b.Put(Encoding.UTF8.GetBytes(Address.ToString()), s.ToArray(), Hub.ResourceFamily);
			
			Hub.Node.Database.Write(b);
		}
	}

	public override string ToString()
	{
		return Address.ToString();
	}
}
