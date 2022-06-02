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
	public enum XonToken
	{
		ChildrenBegin, ChildrenEnd,  NodeBegin, NodeEnd, NameBegin, NameEnd, MetaBegin, MetaEnd, ValueBegin, ValueEnd, SimpleValueBegin, SimpleValueEnd, AttrValueBegin, AttrValueEnd, End
	};

	public interface XonValueSerializator
	{
		object	Set(Xon node, object v);
		string	GetString(Xon node, object v);
		int		GetInt32(object v);
		O		Get<O>(object v) where O : new();
	}

	public interface IXonBinaryMetaSerializator
	{
		byte[]	SerializeMeta(object meta);
		byte[]	SerializeHeader();
		void	DeserializeHeader(byte[] header);
	}

	public class XonTextValueSerializator : XonValueSerializator
	{
		public static readonly XonTextValueSerializator Default = new XonTextValueSerializator();

		public O Get<O>(object v) where O : new()
		{
			throw new NotImplementedException();
		}

		public int GetInt32(object v)
		{
			return int.Parse(v as string);
		}

		public string GetString(Xon node, object v)
		{
			return v as string;
		}

		public object Set(Xon node, object v)
		{
			return v.ToString();
		}
	}

	public class XonBinaryValueSerializator : XonValueSerializator
	{
		public static readonly XonBinaryValueSerializator Default = new XonBinaryValueSerializator();

		public int GetInt32(object v)
		{
			return BitConverter.ToInt32(v as byte[]);
		}

		public virtual string GetString(Xon node, object v)
		{
			return Encoding.UTF8.GetString(v as byte[]);
		}

		public O Get<O>(object v) where O : new()
		{
			var o = new O();
			(o as IBinarySerializable).Read(new BinaryReader(new MemoryStream(v as byte[])));
			return o;
		}

		public virtual object Set(Xon node, object v)
		{
			switch(v)
			{
				case byte[] x:				return x;
				case byte x:				return BitConverter.GetBytes(x);
				case int x:					return BitConverter.GetBytes(x);
				case short x:				return BitConverter.GetBytes(x);
				case long x:				return BitConverter.GetBytes(x);
				case string x:				return Encoding.UTF8.GetBytes(x);
				case IBinarySerializable x:
				{
					var s = new MemoryStream(); 
					var w = new BinaryWriter(s);
					x.Write(w);
					return s.ToArray();
				}
				default:
					throw new NotSupportedException();
			};
		}
	}

	public class XonTypedBinaryValueSerializator : XonBinaryValueSerializator, IXonBinaryMetaSerializator
	{
		List<string> Types = new();

		public override string GetString(Xon node, object value)
		{
			var t = Types[(node.Meta as byte[])[0]];

			var v = value as byte[];

			if(t == typeof(byte[]).FullName)	return v.ToString();
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

			return Hex.ToHexString(v);
		}

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

	public class AsIsXonValueSerializator : XonValueSerializator
	{
		public static readonly AsIsXonValueSerializator Default = new AsIsXonValueSerializator();

		public int GetInt32(object v)
		{
			return (int)v;
		}

		public string GetString(Xon node, object v)
		{
			return v as string;
		}

		public object Set(Xon node, object v)
		{
			return v;
		}

		public O Get<O>(object v) where O : new()
		{
			return (O)v;
		}
	}

	public class Xon// : INestedSerializable
	{
		public string					Name;
		public object					Meta;
		public List<Xon>				Nodes = new List<Xon>();
		public Xon 						Parent;
		
		public XonValueSerializator		Serializator = AsIsXonValueSerializator.Default;
		public object					_Value;
		public object					Value { set => _Value = value is null ? null : (value is Xon ? value : Serializator.Set(this, value)); get => _Value; }

		public string					String => Serializator.GetString(this, _Value);
		public int						Int => Serializator.GetInt32(_Value);

		public List<Xon>				Templates = new List<Xon>();
		public bool						IsTemplate = false;
		public List<Xon>				Removed;
		public bool						IsRemoved = false;

		public Xon()
		{
		}

		public Xon(XonValueSerializator serializator)
		{
			Serializator = serializator;
		}

		public Xon(XonValueSerializator serializator, string name)
		{
			Serializator = serializator;
			Parent		= null;
			Name		= name;
			IsTemplate	= false;
		}

		public Xon Add(string name)
		{
			var n = new Xon(Serializator, name);
			Nodes.Add(n);
			return n;
		}

		public override string ToString()
		{
			return $"{Name}{(_Value != null ? " = " + Serializator.GetString(this, _Value) : null)}";
		}

		public object this[string name]
		{
			get
			{
				var n = One(name);
				
				if(n == null)
					n = Add(name);

				return n.Value;
			}

			set
			{
				var n = One(name);
				
				if(n == null)
					n = Add(name);

				n.Value = value;
			}
		}

		public bool Has(string name)
		{
			return One(name) != null;
		}

		public Xon One(string name)
		{
			var nodes = name.Split(new char[]{ '/'}).ToArray();

			var i = nodes.GetEnumerator();
			
			i.MoveNext();

			//auto p = One(*i);
			var p = Nodes.FirstOrDefault(j => j.Name == i.Current as string);

			if(p != null)
			{
				while(i.MoveNext())
				{
					p = p.Nodes.FirstOrDefault(j => j.Name == (i.Current as string));
					if(p == null)
					{
						return null;
					}
				}
			}
			return p;	 
		}
		
		public List<Xon> Many(string name)
		{
			var o = new List<Xon>();

			var p = this;
			var i = name.LastIndexOf('/');
			var last = name;

			if(i != -1)
			{
				var path = name.Substring(0, i);
				last = name.Substring(i+1);
				p = One(path);
			}

			foreach(var j in p.Nodes)
			{
				if(j.Name == last)
				{
					o.Add(j);
				}
			}

			return o;
		}

		public Xon CloneInternal(Xon parent)
		{
			var p = new Xon(Serializator, Name);
			p.Parent = parent;

			foreach(var i in Nodes)
			{
				p.Nodes.Add(i.CloneInternal(p));
			}

			p.Templates = Templates;

			if(Value != null)
			{
				p.Value = Value;
			}
			return p;
		}

// 		public void Write(ref string b)
// 		{
// 			throw new NotImplementedException();
// 		}
// 
// 		public void Read(string b)
// 		{
// 			throw new NotImplementedException();
// 		}
// 
// 		public object Clone()
// 		{
// 			throw new NotImplementedException();
// 		}

		/// 

		//public IEnumerable<INestedSerializable> GetNodes()
		//{
		//	return Nodes.Cast<INestedSerializable>();
		//}
		//
		//public object GetValue()
		//{
		//	return Value;
		//}
		//
		//public string GetName()
		//{
		//	return Name;
		//}

		public bool IsDifferenceDeleted { get => false; }

		public string	GetString(string name) => One(name).String;
		public int		GetInt32(string name) => One(name).Int;
		public O		Get<O>(string name) where O : new() => Serializator.Get<O>(One(name).Value);
			//public long		GetInt64(string name) => One(name).Value;
	}
}
