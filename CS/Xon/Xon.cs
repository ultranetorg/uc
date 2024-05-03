using System;
using System.Collections.Generic;
using System.Linq;

namespace Uccs
{
	public enum XonToken
	{
		None, ChildrenBegin, ChildrenEnd,  NodeBegin, NodeEnd, NameBegin, NameEnd, MetaBegin, MetaEnd, ValueBegin, ValueEnd, SimpleValueBegin, SimpleValueEnd, AttrValueBegin, AttrValueEnd, End
	};

	public class Xon// : INestedSerializable
	{
		public string					Name;
		public object					Meta;
		public List<Xon>				Nodes = new List<Xon>();
		public Xon 						Parent;
		
		public IXonValueSerializator	Serializator = AsIsXonValueSerializator.Default;
		public object					_Value;
		public object					Value { set => _Value = value is null ? null : (value is Xon ? value : Serializator.Set(this, value)); get => _Value; }

		public string					String => Serializator.Get<string>(this, _Value);
		public int						Int => Serializator.Get<int>(this, _Value);
		public long						Long => Serializator.Get<long>(this, _Value);

		public List<Xon>				Templates = new List<Xon>();
		public bool						IsTemplate = false;
		public List<Xon>				Removed;
		public bool						IsRemoved = false;

		public bool						IsDifferenceDeleted { get => false; }

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
			return $"{Name}{(_Value != null ? " = " + _Value : null)}{(Nodes.Any() ? (Name != null ? ", " : null) + "Nodes=" + Nodes.Count : null)}";
		}

		public object this[string name]
		{
			get
			{
				return One(name).Value;
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

		public Xon One(string path)
		{
			var names = path.Split('/');

			var i = names.GetEnumerator();
			
			i.MoveNext();

			var p = Nodes.FirstOrDefault(j => j.Name.Equals(i.Current));

			if(p != null)
			{
				while(i.MoveNext())
				{
					p = p.Nodes.FirstOrDefault(j => j.Name.Equals(i.Current));

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

		public O Get<O>()
		{
			return Serializator.Get<O>(this, Value);
		} 

		public O GetOr<O>(O otherwise)
		{
			return _Value != null ? Serializator.Get<O>(this, Value) : otherwise;
		} 

		public O Get<O>(string name)
		{
			var n = One(name);

			return Serializator.Get<O>(n, n.Value);
		} 

		public O Get<O>(string name, O otherwise)
		{
			var n = One(name);
	
			if(n != null)
				return Serializator.Get<O>(n, n.Value);
			else
				return otherwise;
		} 
	}
}
