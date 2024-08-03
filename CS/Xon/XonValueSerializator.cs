using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace Uccs
{
	public interface IXonValueSerializator
	{
		object	Set(Xon node, object v);
		O		Get<O>(Xon node, object v) => (O)Get(node, v, typeof(O));
		object	Get(Xon node, object value, Type type);
	}

	public interface IXonBinaryMetaSerializator
	{
		byte[]	SerializeMeta(object meta);
		byte[]	SerializeHeader();
		void	DeserializeHeader(byte[] header);
	}

	public class XonTextValueSerializator : IXonValueSerializator
	{
		public static readonly XonTextValueSerializator Default = new XonTextValueSerializator();

		public virtual object Set(Xon node, object val)
		{
			if(val == null)
			{
				return null;
			}

			if(	val is string			||
				val is byte				||
				val is sbyte			||
				val is short			||
				val is ushort			||
				val is int				||
				val is uint				||
				val is long				||
				val is ulong			||
				val is IPAddress		||
				val is Guid				||
				val.GetType().IsEnum)
				return val.ToString();
			if(val is byte[] ba)	return ba.ToHex();

			throw new NotSupportedException();
		}

		public virtual object Get(Xon node, object value, Type type)
		{
			if(value == null)
				return null;

			var t = value as string;

			if(type == typeof(string))		return value;
			if(type == typeof(byte))		return (object)byte.Parse(t);
			if(type == typeof(sbyte))		return (object)sbyte.Parse(t);
			if(type == typeof(short))		return (object)short.Parse(t);
			if(type == typeof(ushort))		return (object)ushort.Parse(t);
			if(type == typeof(int))			return (object)int.Parse(t);
			if(type == typeof(uint))		return (object)uint.Parse(t);
			if(type == typeof(long))		return (object)long.Parse(t);
			if(type == typeof(ulong))		return (object)ulong.Parse(t);
			if(type == typeof(byte[]))		return (object)t.FromHex();
			if(type.IsEnum)					return Enum.Parse(type, t); 
			if(type == typeof(IPAddress))	return (object)IPAddress.Parse(t);
			if(type == typeof(Guid))		return (object)Guid.Parse(t);

			if(type.GetInterfaces().Any(i => i == typeof(ITextSerialisable)))
			{
				var o = Activator.CreateInstance(type) as ITextSerialisable;
				o.Read(t);
				return o;
			}

			throw new NotSupportedException();
		}
	}

	public class XonBinaryValueSerializator : IXonValueSerializator
	{
		public static readonly XonBinaryValueSerializator Default = new XonBinaryValueSerializator();

		public virtual object Set(Xon node, object v)
		{
			switch(v)
			{
				case byte[] x:	return x;
				case byte x:	return new byte[] {x};
				case short x:	return BitConverter.GetBytes(x);
				case int x:		return BitConverter.GetBytes(x);
				case long x:	return BitConverter.GetBytes(x);
				case string x:	return Encoding.UTF8.GetBytes(x);
				
				case IBinarySerializable x:
				{
					var s = new MemoryStream(); 
					var w = new BinaryWriter(s);
					x.Write(w);
					return s.ToArray();
				}
			};

			throw new NotSupportedException();
		}

		public virtual object Get(Xon node, object value, Type type)
		{
			if(value == null)
				return null;

			var v = value as byte[];

			if(type == typeof(byte[]))	return v;
			if(type == typeof(byte))	return v[0];
			if(type == typeof(short))	return BitConverter.ToInt16(v);
			if(type == typeof(int))		return BitConverter.ToInt32(v);
			if(type == typeof(long))	return BitConverter.ToInt64(v);
			if(type == typeof(string))	return Encoding.UTF8.GetString(v);

			if(type.GetInterfaces().Any(i => i == typeof(IBinarySerializable)))
			{
				var o = Activator.CreateInstance(type) as IBinarySerializable;
				o.Read(new BinaryReader(new MemoryStream(v)));
				return o;
			}

			throw new NotSupportedException();
		}
	}

	public class XonTypedBinaryValueSerializator : XonBinaryValueSerializator, IXonBinaryMetaSerializator
	{
		List<string> Types = new();

		public override object Set(Xon node, object v)
		{
			if(v != null)
			{
				if(!Types.Contains(v.GetType().FullName))
				{
					if(Types.Count == 255)
					{
						throw new NotSupportedException();
					}
	
					Types.Add(v.GetType().FullName);
				} 

				node.Meta = new byte[] {(byte)Types.IndexOf(v.GetType().FullName)};
			}

			return base.Set(node, v);
		}

		public override object Get(Xon node, object value, Type vtype)
		{
			if(vtype == typeof(string))
			{
				var t = Types[(node.Meta as byte[])[0]];
	
				var v = value as byte[];
	
				if(t == typeof(byte[]).FullName)	return v.ToHex();
				if(t == typeof(byte).FullName)		return v[0].ToString();
				if(t == typeof(short).FullName)		return BitConverter.ToInt16(v).ToString();
				if(t == typeof(int).FullName)		return BitConverter.ToInt32(v).ToString();
				if(t == typeof(long).FullName)		return BitConverter.ToInt64(v).ToString();
				if(t == typeof(string).FullName)	return Encoding.UTF8.GetString(v);
				
				var type = Assembly.GetExecutingAssembly().GetType(t);
							
				if(type != null && type.GetInterfaces().Any(i => i == typeof(IBinarySerializable)))
				{
					var o = Activator.CreateInstance(type) as IBinarySerializable;
					o.Read(new BinaryReader(new MemoryStream(v)));
					return o.ToString();
				}
	
				return v.ToHex();
			} 
			else
				return base.Get(node, value, vtype);
		}

		public byte[] SerializeMeta(object m)
		{
			return m as byte[];
		}

		public byte[] SerializeHeader()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(Types.Count);

			foreach(var i in Types)
			{
				w.WriteUtf8(i);
			}

			return s.ToArray();
		}

		public void DeserializeHeader(byte[] header)
		{
			var r = new BinaryReader(new MemoryStream(header));

			var n = r.Read7BitEncodedInt();

			for(int i=0; i<n; i++)
			{
				Types.Add(r.ReadUtf8());			
			}
		}
	}

	public class AsIsXonValueSerializator : IXonValueSerializator
	{
		public static readonly AsIsXonValueSerializator Default = new AsIsXonValueSerializator();

		public object Set(Xon node, object v)
		{
			return v;
		}

		public object Get(Xon node, object value, Type vtype)
		{
			return value;
		}
	}
}
