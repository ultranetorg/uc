using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RocksDbSharp;

namespace Uccs.Net
{
	public class LocalResource
	{
		public Ura		Address { get; set; }
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

		public T LastAs<T>() where T : class  // where T : IBinarySerializable, new()
		{
			//var t = new T();
			//t.Read(new BinaryReader(new MemoryStream(Last)));
			//return t;
			return Last?.Interpretation as T;
		}

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

		public void AddData(DataType type, object interpretation)
		{
			if(Datas == null)
				Datas = new();

			var i = Datas.Find(i => i.Type == type && i.Interpretation.Equals(interpretation));

			if(i != null)
			{
				Datas.Remove(i);
				Datas.Add(i);
			}
			else
			{ 
				Datas.Add(new ResourceData(type, interpretation));
			}
		
			Save();
		}

		internal void Load()
		{
			var d = Hub.Sun.Database.Get(Encoding.UTF8.GetBytes(Address.ToString()), Hub.ResourceFamily);
										
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
				
				Hub.Sun.Database.Write(b);
			}
		}

		public override string ToString()
		{
			return Address.ToString();
		}
	}
}
