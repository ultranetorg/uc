using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using RocksDbSharp;

namespace Uccs.Net
{
	public class ResourceData : IBinarySerializable
	{
		public DataType		Type;
		byte[]				_Data;
		object				_Interpretation;
		ResourceHub			Hub;

		public byte[] Data
		{
			get
			{
				if(_Data == null)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
			
					Write(w);

					_Data = s.ToArray();
				}

				return _Data;
			}
		}
		
		public object Interpretation
		{
			get
			{
				if(_Interpretation == null)
				{
					Read(new BinaryReader(new MemoryStream(Data)));
				}

				return _Interpretation;
			}
		}

		public ResourceData(ResourceHub hub)
		{
			Hub = hub;
		}

		public ResourceData(ResourceHub hub, byte[] data)
		{
			Hub = hub;
			Type = (DataType)new BinaryReader(new MemoryStream(data)).Read7BitEncodedInt();
			_Data = data;
		}

		public ResourceData(ResourceHub hub, BinaryReader reader)
		{
			Hub = hub;
			Read(reader);
		}

		public ResourceData(ResourceHub hub, DataType type, object interpretation)
		{
			Type = type;
			_Interpretation = interpretation;
		}

		public ResourceData(DataType type, byte[] value)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt((int)type);
			w.WriteBytes(value);

			_Data = s.ToArray();
		}

		public static BinaryReader SkipHeader(byte[] data)
		{
			var r = new BinaryReader(new MemoryStream(data));
			r.Read7BitEncodedInt();
			return r;
		}

		public void Read(BinaryReader reader)
		{
			Type = (DataType)reader.Read7BitEncodedInt();
			
			switch(Type)
			{
				case DataType.None:
					_Interpretation = reader.ReadBytes();
					break;

				case DataType.Redirect:
				case DataType.Uri:
					_Interpretation = reader.ReadUtf8();
					break;

				case DataType.IPAddress:
					_Interpretation = new IPAddress(reader.ReadBytes());
					break;

				case DataType.File:
				case DataType.Directory:
					_Interpretation = reader.ReadHash();
					break;

				case DataType.Package: 
					_Interpretation = new History(reader);
					break;

				default:
					throw new ResourceException(ResourceError.UnknownDataType);
			}
		}

		public void Write(BinaryWriter writer)
		{
			if(_Data != null)
			{
				writer.Write(_Data);
			} 
			else
			{
				writer.Write7BitEncodedInt((int)Type);
				
				switch(Type)
				{
					case DataType.None:
						writer.WriteBytes(Interpretation as byte[]);
						break;
	
					case DataType.Redirect:
					case DataType.Uri:
						writer.WriteUtf8(Interpretation as string);
						break;
				
					case DataType.IPAddress:
						writer.WriteBytes((Interpretation as IPAddress).GetAddressBytes());
						break;
				
					case DataType.File:
					case DataType.Directory:
						writer.Write(Interpretation as byte[]);
						break;
				
					case DataType.Package: 
						(Interpretation as History).Write(writer);
						break;
				}
			}
		}
	}

	public class LocalResource
	{
		public ResourceAddress		Address { get; set; }
		public List<ResourceData>	Datas { get; set; }

		public ResourceData			Last => Datas.LastOrDefault();

		ResourceHub					Hub;

		public LocalResource()
		{
		}

		public LocalResource(ResourceHub hub, ResourceAddress resource)
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

		public void AddData(byte[] data)
		{
			if(Datas == null)
				Datas = new();

			var i = Datas.Find(i => i.Data.SequenceEqual(data));

			if(i != null)
			{
				Datas.Remove(i);
				Datas.Add(i);
			}
			else
			{
				Datas.Add(new ResourceData(Hub, data));
			}
			
			Save();
		}

		public void AddData(DataType type, object interpretation)
		{
			if(Datas == null)
				Datas = new();

			var d = new ResourceData(Hub, type, interpretation);

			var i = Datas.Find(i => i.Data.SequenceEqual(d.Data));

			if(i != null)
			{
				Datas.Remove(i);
				Datas.Add(i);
			}
			else
			{ 
				Datas.Add(d);
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
	
				Datas = r.ReadList<ResourceData>(() => new ResourceData(Hub, r));
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
