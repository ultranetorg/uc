﻿using System.Collections.Generic;
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

		ResourceHub					Hub;

		public LocalResource()
		{
		}

		public LocalResource(ResourceHub hub, ResourceAddress resource)
		{
			Hub = hub;
			Address = resource;
		}

		public T DataAs<T>() where T : IBinarySerializable, new()
		{
			var t = new T();
			t.Read(new BinaryReader(new MemoryStream(Last)));
			return t;
		}

		public void AddData(byte[] data)
		{
			if(Datas == null)
				Datas = new();

			var i = Datas.FindIndex(i => i.SequenceEqual(data));

			if(i != -1)
			{
				Datas.RemoveAt(i);
				Datas.Add(data);
				return;
			}

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