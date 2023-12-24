using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public class BinarySerializator
	{
		public static void Serialize(BinaryWriter writer, object o)
		{
			if(o is ITypedBinarySerializable t)
			{
				writer.Write(t.TypeCode);
			}

			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				var val = i.GetValue(o);

				if(!Serialize(writer, val, i.PropertyType))
					Serialize(writer, val);
			}
		}

		static bool Serialize(BinaryWriter writer, object val, Type type)
		{
			switch(val)
			{
				case bool v :	writer.Write(v); return true;
				case byte v :	writer.Write(v); return true;
				case int v :	writer.Write7BitEncodedInt(v); return true;
				case long v :	writer.Write7BitEncodedInt64(v); return true;
				case Money v :	v.Write(writer); return true;
			}

			if(type.IsEnum)
			{
				if(Enum.GetUnderlyingType(type) == typeof(byte))
					writer.Write((byte)val);
				else
					writer.Write7BitEncodedInt((int)val);
				return true;
			}

			writer.Write(val != null); /// null or not?

			if(val == null)
			{
				return true;
			}
				
			switch(val)
			{
				case string v:
					writer.WriteUtf8(v);
					return true;

				case byte[] v:
					writer.Write7BitEncodedInt(v.Length);
					writer.Write(v);
					return true;

				case IPAddress v:
				{
					writer.Write(v);
					return true;
				}
				case IBinarySerializable v:
					v.Write(writer);
					return true;

	 			case Operation v:
	 				writer.Write((byte)v.Class);
	 				v.Write(writer);
					return true;

	 			case XonDocument v:
				{	
					var s = new MemoryStream();
					v.Save(new XonBinaryWriter(s));
			
					var b = s.ToArray();
						
					writer.Write7BitEncodedInt(b.Length);
					writer.Write(b);
					return true;
				}
				case System.Collections.ICollection v:
				{
					writer.Write7BitEncodedInt(v.Count);
							
					foreach(var j in v)
					{
						if(!Serialize(writer, j, type.GetElementType()))
							Serialize(writer, j);
					}
					return true;
				}
			}

			return false;
		}


		public static void Serialize(BinaryWriter writer, IEnumerable<object> many)
		{
			writer.Write7BitEncodedInt(many.Count());
	
			foreach(var i in many)
			{
				Serialize(writer, i);
			}
		}

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

		static object Construct(BinaryReader reader, Type type, Func<Type, byte, object> construct, Action<object> initialize)
		{
			object o;

			if(type.GetInterfaces().Contains(typeof(ITypedBinarySerializable)))
				o = construct(type, reader.ReadByte());
			else
				o = construct(type, 0) ?? type.GetConstructor(new System.Type[]{}).Invoke(null);

			initialize?.Invoke(o);

			return o;
		}

		public static O Deserialize<O>(BinaryReader reader, Func<Type, byte, object> construct, Action<object> initialize = null) where O : class
		{
			return Deserialize(reader, typeof(O), construct, initialize) as O;
		}

		public static object Deserialize(BinaryReader reader, Type type, Func<Type, byte, object> construct, Action<object> initialize = null)
		{
			var o = Construct(reader, type, construct, initialize);

			foreach(var p in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				if(DeserializeValue(reader, p.PropertyType, construct, initialize, out object val))
					p.SetValue(o, val);
				else
					p.SetValue(o, Deserialize(reader, p.PropertyType, construct, initialize));
			}

			return o;
		}

		public static O[] DeserializeMany<O>(BinaryReader reader, Func<Type, byte, object> construct, Action<object> initialize = null) where O : ITypedBinarySerializable
		{
			var n = reader.Read7BitEncodedInt();
			var l = new O[n];

			for(int i=0; i<n; i++)
			{
				var o = Construct(reader,  typeof(O), construct, initialize);

				foreach(var p in l[i].GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
				{
					if(DeserializeValue(reader, p.PropertyType, construct, initialize, out object val))
						p.SetValue(l[i], val);
					else
						p.SetValue(l[i], Deserialize(reader, p.PropertyType, construct, initialize));
				}
			}

			return l;
		}

		static bool DeserializeValue(BinaryReader reader, Type type, Func<Type, byte, object> construct, Action<object> initialize, out object value)
		{
			if(typeof(bool) == type)			
			{
				value = reader.ReadBoolean();
				return true;
			}
			else if(typeof(byte) == type)
			{	
				value = reader.ReadByte();
				return true;
			}
			else if(typeof(int) == type)
			{	
				value = reader.Read7BitEncodedInt(); 
				return true;
			}
			else if(typeof(long) == type)		
			{
				value = reader.Read7BitEncodedInt64(); 
				return true;
			}
			else if(typeof(Money) == type)
			{
				var c = new Money(); 
				c.Read(reader);
				value = c;
				return true;
			}
			else if(type.IsEnum)
			{
				if(Enum.GetUnderlyingType(type) == typeof(byte))
					value = Enum.ToObject(type, reader.ReadByte()); 
				else
					value = Enum.ToObject(type, reader.Read7BitEncodedInt()); 
				
				return true;
			}

			if(reader.ReadBoolean())
			{
				if(typeof(string) == type)
				{
					value = reader.ReadUtf8(); 
					return true;
				} 
				else
				if(typeof(byte[]) == type)
				{ 
					value = reader.ReadBytes(reader.Read7BitEncodedInt()); 
					return true;
				}
				else
				if(typeof(IPAddress) == type)
				{
					value = reader.ReadIPAddress();
					return true;
				}
				else
				if(type == typeof(XonDocument))
				{ 
					value = new XonDocument(new XonBinaryReader(new MemoryStream(reader.ReadBytes(reader.Read7BitEncodedInt()))), new XonTypedBinaryValueSerializator());
					return true;
				}
				else
				if(type == typeof(Operation) || type.IsSubclassOf(typeof(Operation)))
				{ 
					var t = (OperationClass)reader.ReadByte();
					var o = Operation.FromType(t);
					o.Read(reader);
					value = o;
					return true;
				}
				else
				if(type.GetInterfaces().Any(i => i == typeof(IBinarySerializable))) 
				{
					var o = Construct(reader, type, construct, initialize);
					
					(o as IBinarySerializable).Read(reader);
					value = o;
					return true;
				}
				else 
				if(type.GetInterfaces().Any(i => i == typeof(ICollection)))
				{
					var ltype = type.GetElementType().MakeArrayType(1);
	
					var n = reader.Read7BitEncodedInt();
	
					var l = ltype.GetConstructor(new System.Type[]{typeof(int)}).Invoke(new object[]{n});
	
					for(int i=0; i<n; i++)
					{
						if(DeserializeValue(reader, type.GetElementType(), construct, initialize, out object v))
							l.GetType().GetMethod("Set").Invoke(l, new object[]{i, v});
							//l[i] = v; 
						else
							l.GetType().GetMethod("Set").Invoke(l, new object[]{i, Deserialize(reader, type.GetElementType(), construct)});
					}

					value = l;
					return true;
				}
				else
				{
					value = null;
					return false;
				}
			}
			else
			{
				value = null;
				return true;
			}
		}
	}
}
