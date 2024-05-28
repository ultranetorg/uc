using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Uccs.Net
{
	public class BinarySerializator
	{
		public static void Serialize(BinaryWriter writer, object o)
		{
			Serialize(writer, o, o.GetType());
		}

		static void Serialize(BinaryWriter writer, object val, Type type)
		{
			switch(val)
			{
				case bool v :	writer.Write(v); return;
				case byte v :	writer.Write(v); return;
				case short v :	writer.Write(v); return;
				case int v :	writer.Write7BitEncodedInt(v); return;
				case long v :	writer.Write7BitEncodedInt64(v); return;
				case Money v :	writer.Write(v); return;
				case Guid v :	writer.Write(v.ToByteArray()); return;
			}

			if(type.IsEnum)
			{
				if(Enum.GetUnderlyingType(type) == typeof(byte))
					writer.Write((byte)val);
				else
					writer.Write7BitEncodedInt((int)val);
				return;
			}

			writer.Write(val != null); /// null or not?

			if(val == null)
			{
				return;
			}
				
			switch(val)
			{
				case string v:
					writer.WriteUtf8(v);
					return;

				case byte[] v:
					writer.Write7BitEncodedInt(v.Length);
					writer.Write(v);
					return;

				case IPAddress v:
				{
					writer.Write(v);
					return;
				}

	 			case XonDocument v:
				{	
					var s = new MemoryStream();
					v.Save(new XonBinaryWriter(s));
			
					var b = s.ToArray();
						
					writer.Write7BitEncodedInt(b.Length);
					writer.Write(b);
					return;
				}
				case ICollection v:
				{
					writer.Write7BitEncodedInt(v.Count);
							
					foreach(var j in v)
					{
						Serialize(writer, j, type.GetElementType());
					}
					return;
				}
			}

			if(val is ITypeCode t)
			{
				writer.Write(t.TypeCode);
			}

			if(val is IBinarySerializable bs)
			{
				bs.Write(writer);
			} 
			else
			{
				foreach(var i in type.GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
				{
					Serialize(writer, i.GetValue(val), i.PropertyType);
				}
			}
		}

//		public static void Serialize(BinaryWriter writer, IEnumerable<object> many)
//		{
//			writer.Write7BitEncodedInt(many.Count());
//	
//			foreach(var i in many)
//			{
//				Serialize(writer, i);
//			}
//		}

// 		public static object Deserialize(BinaryReader reader, Type type, Func<Type, object> construct)
// 		{
// 			object o = null;
// 
// 			if(construct != null)
// 				o = construct(type);
// 
// 			if(o == null)
// 				o = type.GetConstructor(new System.Type[]{}).Invoke(new object[]{});
// 
// 			foreach(var p in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
// 			{
// 				if(DeserializeValue(reader, p.PropertyType, construct, out object val))
// 					p.SetValue(o, val);
// 				else
// 					p.SetValue(o, Deserialize(reader, p.PropertyType, construct));
// 			}
// 
// 			return o;
// 		}

		static object Construct(BinaryReader reader, Type type, Func<Type, byte, object> construct)
		{
			object o;

			if(type.GetInterfaces().Contains(typeof(ITypeCode)))
				o = construct(type, reader.ReadByte());
			else
				o = construct(type, 0) ?? type.GetConstructor(new System.Type[]{}).Invoke(null);

			//initialize?.Invoke(o);

			return o;
		}

		public static O Deserialize<O>(BinaryReader reader, Func<Type, byte, object> construct)
		{
			return (O)Deserialize(reader, typeof(O), construct);
		}

		//public static O[] DeserializeMany<O>(BinaryReader reader, Func<Type, byte, object> construct, Action<object> initialize = null) where O : ITypedBinarySerializable
		//{
		//	var n = reader.Read7BitEncodedInt();
		//	var l = new O[n];
		//
		//	for(int i=0; i<n; i++)
		//	{
		//		var o = Construct(reader,  typeof(O), construct, initialize);
		//
		//		foreach(var p in l[i].GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
		//		{
		//			if(DeserializeValue(reader, p.PropertyType, construct, initialize, out object val))
		//				p.SetValue(l[i], val);
		//			else
		//				p.SetValue(l[i], Deserialize(reader, p.PropertyType, construct, initialize));
		//		}
		//	}
		//
		//	return l;
		//}

		static object Deserialize(BinaryReader reader, Type type, Func<Type, byte, object> construct)
		{
			if(typeof(bool) == type)			
			{
				return reader.ReadBoolean();
			}
			else if(typeof(byte) == type)
			{	
				return reader.ReadByte();
			}
			else if(typeof(short) == type)
			{	
				return reader.ReadInt16(); 
			}
			else if(typeof(int) == type)
			{	
				return reader.Read7BitEncodedInt(); 
			}
			else if(typeof(long) == type)		
			{
				return reader.Read7BitEncodedInt64(); 
			}
			else if(typeof(Money) == type)
			{
				var c = new Money(); 
				c.Read(reader);
				return c;
			}
			else if(typeof(Guid) == type)
			{
				return new Guid(reader.ReadBytes(16));
			}
			else if(type.IsEnum)
			{
				if(Enum.GetUnderlyingType(type) == typeof(byte))
					return Enum.ToObject(type, reader.ReadByte()); 
				else
					return Enum.ToObject(type, reader.Read7BitEncodedInt()); 
			}

			if(!reader.ReadBoolean())
				return null;
			
			if(typeof(byte[]) == type)		
				return reader.ReadBytes(reader.Read7BitEncodedInt()); 
			else if(typeof(string) == type)
				return reader.ReadUtf8(); else 
			if(typeof(IPAddress) == type)
				return reader.ReadIPAddress();
			else if(type == typeof(XonDocument))
				return new XonDocument(new XonBinaryReader(new MemoryStream(reader.ReadBytes(reader.Read7BitEncodedInt()))), new XonTypedBinaryValueSerializator());
			else if(type.GetInterfaces().Any(i => i == typeof(ICollection)))
			{
				var ltype = type.GetElementType().MakeArrayType(1);
	
				var n = reader.Read7BitEncodedInt();
	
				var l = ltype.GetConstructor(new Type[] {typeof(int)}).Invoke(new object[]{n});
	
				for(int i=0; i<n; i++)
				{
					l.GetType().GetMethod("Set").Invoke(l, new object[]{i, Deserialize(reader, type.GetElementType(), construct)});
				}

				return l;
			}

			var o = Construct(reader, type, construct);

			if(type.GetInterfaces().Any(i => i == typeof(IBinarySerializable))) 
			{
				(o as IBinarySerializable).Read(reader);
			}
			else
				foreach(var p in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
				{
					p.SetValue(o, Deserialize(reader, p.PropertyType, construct));
				}

			return o;
		}
	}
}
