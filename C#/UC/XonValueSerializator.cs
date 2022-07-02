using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Utilities.Encoders;
using UC.Net;

namespace UC
{
	public interface IXonValueSerializator
	{
		object	Set(Xon node, object v);
		O		Get<O>(Xon node, object v);
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

		public O Get<O>(Xon node, object value)
		{
			var v = value as string;

			if(typeof(O) == typeof(int))	return (O)(object)int.Parse(v);
			if(typeof(O) == typeof(long))	return (O)(object)long.Parse(v);
			if(typeof(O) == typeof(string))	return (O)(object)v;

			throw new NotSupportedException();
		}

		public object Set(Xon node, object v)
		{
			return v.ToString();
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
				case byte x:	return BitConverter.GetBytes(x);
				case int x:		return BitConverter.GetBytes(x);
				case short x:	return BitConverter.GetBytes(x);
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

		public virtual O Get<O>(Xon node, object value)
		{
			var v = value as byte[];

			if(typeof(O) == typeof(byte[]))	return (O)(object)v;
			if(typeof(O) == typeof(byte))	return (O)(object)v[0];
			if(typeof(O) == typeof(short))	return (O)(object)BitConverter.ToInt16(v);
			if(typeof(O) == typeof(int))	return (O)(object)BitConverter.ToInt32(v);
			if(typeof(O) == typeof(long))	return (O)(object)BitConverter.ToInt64(v);
			if(typeof(O) == typeof(string))	return (O)(object)Encoding.UTF8.GetString(v);

			if(typeof(O).GetInterfaces().Any(i => i == typeof(IBinarySerializable)))
			{
				var o = Activator.CreateInstance(typeof(O)) as IBinarySerializable;
				o.Read(new BinaryReader(new MemoryStream(v)));
				return (O)o;
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

		public override O Get<O>(Xon node, object value)
		{
			if(typeof(O) == typeof(string))
			{
				var t = Types[(node.Meta as byte[])[0]];
	
				var v = value as byte[];
	
				if(t == typeof(byte[]).FullName)	return (O)(object)v.ToString();
				if(t == typeof(byte).FullName)		return (O)(object)v[0].ToString();
				if(t == typeof(short).FullName)		return (O)(object)BitConverter.ToInt16(v).ToString();
				if(t == typeof(int).FullName)		return (O)(object)BitConverter.ToInt32(v).ToString();
				if(t == typeof(long).FullName)		return (O)(object)BitConverter.ToInt64(v).ToString();
				if(t == typeof(string).FullName)	return (O)(object)Encoding.UTF8.GetString(v);
				
				var type = Assembly.GetExecutingAssembly().GetType(t);
							
				if(type != null && type.GetInterfaces().Any(i => i == typeof(IBinarySerializable)))
				{
					var o = Activator.CreateInstance(type) as IBinarySerializable;
					o.Read(new BinaryReader(new MemoryStream(v)));
					return (O)(object)o.ToString();
				}
	
				return (O)(object)Hex.ToHexString(v);
			} 
			else
				return base.Get<O>(node, value);
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

		public O Get<O>(Xon node, object v)
		{
			return (O)v;
		}
	}
}
