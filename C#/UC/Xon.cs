using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC
{
	public enum XonToken
	{
		ChildrenBegin, ChildrenEnd,  NodeBegin, NodeEnd, NameBegin, NameEnd, ValueBegin, ValueEnd, SimpleValueBegin, SimpleValueEnd, AttrValueBegin, AttrValueEnd, End
	};

	public interface IXonValueSerializator
	{
		object	Set(object v);
		string	GetString(object v);
		int		GetInt32(object v);
	}

	public class TextXonValueSerializator : IXonValueSerializator
	{
		public int GetInt32(object v)
		{
			return int.Parse(v as string);
		}

		public string GetString(object v)
		{
			return v as string;
		}

		public object Set(object v)
		{
			return v switch
					{ 
						string => v,
						int i => i.ToString(),
						_ => throw new NotSupportedException()
					};
		}
	}

	public class AsIsXonValueSerializator : IXonValueSerializator
	{
		public static readonly AsIsXonValueSerializator Default = new AsIsXonValueSerializator();

		public int GetInt32(object v)
		{
			return (int)v;
		}

		public string GetString(object v)
		{
			return v as string;
		}

		public object Set(object v)
		{
			return v;
		}
	}

	public class Xon// : INestedSerializable
	{
		public string					Name;
		public string					Type = "";
		public List<Xon>				Nodes = new List<Xon>();
		public Xon 						Parent;
		
		protected IXonValueSerializator	Serializator = AsIsXonValueSerializator.Default;
		object							_Value;
		public object					Value { set => _Value = value is null ? null : (value is Xon ? value : Serializator.Set(value)); get => _Value; }

		public string					String => Serializator.GetString(_Value);
		public int						Int => Serializator.GetInt32(_Value);

		public List<Xon>				Templates = new List<Xon>();
		public bool						IsTemplate = false;
		public List<Xon>				Removed;
		public bool						IsRemoved = false;

		public Xon()
		{
		}

		public Xon(IXonValueSerializator serializator)
		{
			Serializator = serializator;
		}

		public Xon(IXonValueSerializator serializator, string name)
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
			return $"{Name}{(_Value != null ? " = " + Serializator.GetString(_Value) : null)}";
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

		public void Write(ref string b)
		{
			throw new NotImplementedException();
		}

		public void Read(string b)
		{
			throw new NotImplementedException();
		}

		public object Clone()
		{
			throw new NotImplementedException();
		}

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
		//public long		GetInt64(string name) => One(name).Value;
	}
}
