using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RocksDbSharp;

namespace Uccs.Net
{
	public class LocalResource
	{
		public ResourceAddress		Address { get; set; }
		public List<byte[]>			Datas { get; set; }

		public byte[]				Last => Datas.Last();
		public History				LastAsHistory => new History(Datas.Last());

		ResourceHub					Hub;

		public LocalResource(ResourceHub hub, ResourceAddress resource)
		{
			Hub = hub;
			Address = resource;
		}

		public void AddData(byte[] data)
		{
			if(Datas == null)
				Datas = new();

			Datas.Add(data);
			Save();
		}

		internal void Load()
		{
			var d = Hub.Sun.Database.Get(Encoding.UTF8.GetBytes(Address.ToString()), Hub.ResourceFamily);
										
			if(d != null)
			{
				var s = new MemoryStream(d);
				var r = new BinaryReader(s);
	
				Datas = r.ReadList(() => r.ReadBytes());
			}
		}

		internal void Save()
		{
			using(var b = new WriteBatch())
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
				
				w.Write(Datas, i => w.WriteBytes(i));

				b.Put(Encoding.UTF8.GetBytes(Address.ToString()), s.ToArray(), Hub.ResourceFamily);
				
				Hub.Sun.Database.Write(b);
			}
		}
	}
}
