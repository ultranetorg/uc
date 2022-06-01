using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public interface IBinarySerializable
	{
		void Read(BinaryReader r);
		void Write(BinaryWriter w);
	}

	public interface ITypedBinarySerializable
	{
		byte BinaryType { get; }
	}

	public class BinarySerializator
	{
		public static void Serialize(BinaryWriter writer, object o)
		{
			foreach(var i in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
			{
				var val = i.GetValue(o);

				switch(val)
				{
					case int v:
						writer.Write7BitEncodedInt(v);
						continue;

					case long v:
						writer.Write7BitEncodedInt64(v);
						continue;
				}

				if(i.PropertyType.IsEnum)
				{
					writer.Write7BitEncodedInt((int)i.GetValue(o));
					continue;
				}

				writer.Write(val != null);

				if(val == null)
				{
					continue;
				}
				
				switch(val)
				{
					case string v:
						writer.WriteUtf8(v);
						continue;

					case byte[] v:
						writer.Write7BitEncodedInt(v.Length);
						writer.Write(v);
						continue;

					case IPAddress v:
						var b = v.GetAddressBytes();
						writer.Write7BitEncodedInt(b.Length);
						writer.Write(b);
						continue;

					case IBinarySerializable v:
						writer.Write(v);
						continue;

	 				case Operation v:
	 					writer.Write((byte)v.Type);
	 					v.Write(writer);
						continue;

					case System.Collections.IEnumerable v:
						writer.Write7BitEncodedInt((v as IEnumerable<object>).Count());
							
						foreach(var j in v)
						{
							Serialize(writer, j);
						}
						continue;

					default:
						throw new NotSupportedException();
				}
			}
		}


		public static void Serialize(BinaryWriter writer, IEnumerable<object> many)
		{
			writer.Write7BitEncodedInt(many.Count());
	
			foreach(var i in many)
			{
				if(i is ITypedBinarySerializable t)
				{
					writer.Write(t.BinaryType);
				}

				Serialize(writer, i);
			}
		}

		public static O[] Deserialize<O>(BinaryReader reader, Func<int, O> fromtype) where O : ITypedBinarySerializable
		{
			var n = reader.Read7BitEncodedInt();
			var l = new O[n];

			for(int i=0; i<n; i++)
			{
				l[i] = fromtype(reader.ReadByte());

				foreach(var p in l[i].GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
				{
					p.SetValue(l[i], DeserializeProperty(reader, p.PropertyType));
				}
			}

			return l;
		}

		public static O Deserialize<O>(BinaryReader reader) where O : class
		{
			return Deserialize(reader, typeof(O)) as O;
		}


		public static object Deserialize(BinaryReader reader, Type type)
		{
			var o = type.GetConstructor(new System.Type[]{}).Invoke(new object[]{});

			foreach(var p in o.GetType().GetProperties().Where(i => i.CanRead && i.CanWrite))
			{
				p.SetValue(o, DeserializeProperty(reader, p.PropertyType));
			}

			return o;
		}

		static object DeserializeProperty(BinaryReader reader, Type type)
		{
			if(typeof(int) == type)			
				return reader.Read7BitEncodedInt(); else 
			if(typeof(long) == type)		
				return reader.Read7BitEncodedInt64(); else
			if(type.IsEnum)
				return Enum.ToObject(type, reader.Read7BitEncodedInt()); 

			if(reader.ReadBoolean())
			{
				if(typeof(string) == type)		return reader.ReadUtf8(); 
				else
				if(typeof(byte[]) == type)		return reader.ReadBytes(reader.Read7BitEncodedInt()); 
				else
				if(typeof(IPAddress) == type)	return new IPAddress(reader.ReadBytes(reader.Read7BitEncodedInt())); 
				else
				if(type == typeof(Operation) || type.IsSubclassOf(typeof(Operation)))
				{ 
					var t = (Operations)reader.ReadByte();
					var o = Operation.FromType(t);
					o.Read(reader);
					return o;
				}
				else
				if(type.GetInterfaces().Any(i => i == typeof(IBinarySerializable))) 
				{
					var x = type.GetConstructor(new System.Type[]{}).Invoke(new object[]{}) as IBinarySerializable;
					x.Read(reader);
					return x;
				}
				else 
				if(type.GetInterfaces().Any(i => i == typeof(System.Collections.IEnumerable)))
				{
					var ltype = type.GetGenericArguments()[0].MakeArrayType(1);
	
					var n = reader.Read7BitEncodedInt();
	
					var l = ltype.GetConstructor(new System.Type[]{typeof(int)}).Invoke(new object[]{n}) as object[];
	
	
					for(int i=0; i<n; i++)
					{
						l[i] = Deserialize(reader, type.GetGenericArguments()[0]);
						//ltype.GetProperties(System.Reflection.BindingFlags.SetProperty) .Invoke(l, new object[] {i, e} );
					}
	
					return l;
				}
				else
					throw new NotSupportedException();
			} 
			else
				return null;
		}
	}
}
