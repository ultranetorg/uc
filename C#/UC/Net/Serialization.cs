using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public interface ITypedBinarySerializable
	{
		byte TypeCode { get; }
	}

	public class BinarySerializator
	{
		public static void Serialize(BinaryWriter writer, object o)
		{
			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
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
				case bool v:
					writer.Write(v);
					return true;

				case int v:
					writer.Write7BitEncodedInt(v);
					return true;

				case long v:
					writer.Write7BitEncodedInt64(v);
					return true;

				case Coin v:
					v.Write(writer);
					return true;
			}

			if(type.IsEnum)
			{
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
					var b = v.GetAddressBytes();
					writer.Write7BitEncodedInt(b.Length);
					writer.Write(b);
					return true;
				}
				case IBinarySerializable v:
					v.Write(writer);
					return true;

	 			case Operation v:
	 				writer.Write((byte)v.Type);
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
				case System.Collections.IEnumerable v:

					var n = 0;
					foreach(var i in (System.Collections.IEnumerable)v)
					{
						n++;
					}

					writer.Write7BitEncodedInt(n);
							
					foreach(var j in v)
					{
						if(!Serialize(writer, j, type.GetGenericArguments()[0]))
							Serialize(writer, j);
					}
					return true;
			}

			return false;
		}


		public static void Serialize(BinaryWriter writer, IEnumerable<object> many)
		{
			writer.Write7BitEncodedInt(many.Count());
	
			foreach(var i in many)
			{
				if(i is ITypedBinarySerializable t)
				{
					if(t.TypeCode == 13)
						writer=writer;
					
					writer.Write(t.TypeCode);
				}

				Serialize(writer, i);
			}
		}

		public static O[] Deserialize<O>(BinaryReader reader, Func<int, O> fromtype, Func<Type, object> construct) where O : ITypedBinarySerializable
		{
			var n = reader.Read7BitEncodedInt();
			var l = new O[n];

			for(int i=0; i<n; i++)
			{
				var t = reader.ReadByte();
				
				if(t == 13)
					t=t;

				l[i] = fromtype(t);

				foreach(var p in l[i].GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
				{
					if(DeserializeValue(reader, p.PropertyType, construct, out object val))
						p.SetValue(l[i], val);
					else
						p.SetValue(l[i], Deserialize(reader, p.PropertyType, construct));
				}
			}

			return l;
		}

		public static O Deserialize<O>(BinaryReader reader, Func<Type, object> construct) where O : class
		{
			return Deserialize(reader, typeof(O), construct) as O;
		}

		public static object Deserialize(BinaryReader reader, Type type, Func<Type, object> construct)
		{
			object o = null;

			if(construct != null)
				o = construct(type);

			if(o == null)
				o = type.GetConstructor(new System.Type[]{}).Invoke(new object[]{});

			foreach(var p in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
			{
				if(DeserializeValue(reader, p.PropertyType, construct, out object val))
					p.SetValue(o, val);
				else
					p.SetValue(o, Deserialize(reader, p.PropertyType, construct));
			}

			return o;
		}

		static bool DeserializeValue(BinaryReader reader, Type type, Func<Type, object> construct, out object value)
		{
			if(typeof(bool) == type)			
			{
				value = reader.ReadBoolean();
				return true;
			}
			else 
			if(typeof(int) == type)
			{	
				value = reader.Read7BitEncodedInt(); 
				return true;
			}
			else 
			if(typeof(long) == type)		
			{
				value = reader.Read7BitEncodedInt64(); 
				return true;
			}
			else 
			if(typeof(Coin) == type)
			{
				var c = new Coin(); 
				c.Read(reader);
				value = c;
				return true;
			}
			else
			if(type.IsEnum)
			{
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
					value = new IPAddress(reader.ReadBytes(reader.Read7BitEncodedInt()));
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
					var t = (Operations)reader.ReadByte();
					var o = Operation.FromType(t);
					o.Read(reader);
					value = o;
					return true;
				}
				else
				if(type.GetInterfaces().Any(i => i == typeof(IBinarySerializable))) 
				{
					object o = null;

					if(construct != null)
						o = construct(type);

					if(o == null)
						o = type.GetConstructor(new System.Type[]{}).Invoke(new object[]{});
					
					(o as IBinarySerializable).Read(reader);
					value = o;
					return true;
				}
				else 
				if(type.GetInterfaces().Any(i => i == typeof(System.Collections.IEnumerable)))
				{
					var ltype = type.GetGenericArguments()[0].MakeArrayType(1);
	
					var n = reader.Read7BitEncodedInt();
	
					var l = ltype.GetConstructor(new System.Type[]{typeof(int)}).Invoke(new object[]{n});
	
					for(int i=0; i<n; i++)
					{
						if(DeserializeValue(reader, type.GetGenericArguments()[0], construct, out object v))
							l.GetType().GetMethod("Set").Invoke(l, new object[]{i, v});
							//l[i] = v; 
						else
							l.GetType().GetMethod("Set").Invoke(l, new object[]{i, Deserialize(reader, type.GetGenericArguments()[0], construct)});
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
