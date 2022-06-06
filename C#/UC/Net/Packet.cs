using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;

namespace UC.Net
{
	public class Packet
	{
		//public Header			Header;
		public PacketType		Type;
		public MemoryStream		Data;

		public Packet()
		{
		}

		public Packet(PacketType type, MemoryStream data)
		{
			Type = type;
			Data = data;
		}

		public static Packet Create<T>(PacketType type, IEnumerable<T> many) where T : IBinarySerializable
		{
			if(many.Count() > 0)
			{
				var s = new MemoryStream();
				var w = new BinaryWriter(s);
	
				w.Write7BitEncodedInt(many.Count());
	
				foreach(var i in many)
				{
					if(i is ITypedBinarySerializable t)
						w.Write(t.TypeCode);

					i.Write(w);
				}
	
				return new Packet(type, s);
			}
			else
				return null;
		}

		public C[] Read<C>(Func<BinaryReader, byte, C> construct) where C : IBinarySerializable
		{
			if(Data != null)
			{
				var r = new BinaryReader(Data);
	
				var n = r.Read7BitEncodedInt();
	
				var many = new C[n];
	
				for(int i=0; i<n; i++)
				{
					var o = construct(r, r.ReadByte());
					o.Read(r);
					many[i] = o;
				}
	
				return many;
			} 
			else
			{
				return new C[0];
			}
		}

		public T[] Read<T>(Func<BinaryReader, T> construct) where T : IBinarySerializable
		{
			if(Data != null)
			{
				var r = new BinaryReader(Data);
	
				var n = r.Read7BitEncodedInt();
	
				var many = new T[n];
	
				for(int i=0; i<n; i++)
				{
					var o = construct(r);
					o.Read(r);
					many[i] = o;
				}
	
				return many;
			} 
			else
			{
				return new T[0];
			}
		}

		IEnumerable<O> ReadMany<O>(Func<BinaryReader, O> read)
		{
			if(Data != null)
			{
				var r = new BinaryReader(Data);
	
				var n = r.Read7BitEncodedInt();
	
				for(int i=0; i<n; i++)
				{
					yield return read(r);
					
				}
			} 
		}
// 
// 		public static MemoryStream Write<T>(IEnumerable<T> many) where T : IBinarySerializable
// 		{
// 			if(many.Count() > 0)
// 			{
// 				var s = new MemoryStream();
// 				var w = new BinaryWriter(s);
// 	
// 				w.Write7BitEncodedInt(many.Count());
// 	
// 				foreach(var i in many)
// 				{
// 					if(i is ITypedBinarySerializable t)
// 						w.Write(t.TypeCode);
// 
// 					i.Write(w);
// 				}
// 	
// 				return s;
// 			}
// 			else
// 				return null;
// 		}
// 
// 		MemoryStream Write<T>(IEnumerable<T> many, Action<BinaryWriter, T> write)
// 		{
// 			if(many.Count() > 0)
// 			{
// 				var s = new MemoryStream();
// 				var w = new BinaryWriter(s);
// 	
// 				w.Write7BitEncodedInt(many.Count());
// 	
// 				foreach(var i in many)
// 				{
// 					write(w, i);
// 				}
// 	
// 				return s;
// 			}
// 			else
// 				return null;
// 		}
	}
}