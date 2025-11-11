using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs;

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

	public Xon(IXonValueSerializator serializator, string name) : this(serializator)
	{
		Parent		= null;
		Name		= name;
		IsTemplate	= false;
	}

	public Xon(string text) : this(XonTextValueSerializator.Default)
	{
		Load(null, new XonTextReader(text));
	}

	public Xon(string text, IXonValueSerializator serializator) : this(serializator)
	{
		Load(null, new XonTextReader(text));
	}

	public Xon(byte[] data) : this(XonBinaryValueSerializator.Default)
	{
		Load(null, new XonBinaryReader(new MemoryStream(data)));
	}
	
	public Xon(IXonReader r, IXonValueSerializator serializator) : this(serializator)
	{
		Load(null, r);
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

	public object Get(Type type)
	{
		return Serializator.Get(this, Value, type);
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

	public object Get(Type type, string name)
	{
		var n = One(name);

		return Serializator.Get(n, n.Value, type);
	} 

	public O Get<O>(string name, O otherwise)
	{
		var n = One(name);

		if(n != null)
			return Serializator.Get<O>(n, n.Value);
		else
			return otherwise;
	} 

	public E GetEnum<E>(string name)
	{
		var n = One(name);

		return (E)Serializator.Get(n, n.Value, typeof(E));
	} 

	public E GetEnum<E>(string name, E otherwise)
	{
		var n = One(name);

		if(n != null)
			return (E)Serializator.Get(n, n.Value, typeof(E));
		else
			return otherwise;
	} 

	internal void Load(Xon t, IXonReader r)
	{
		r.Read(Serializator);

		Xon n;

		while(r.Current != XonToken.End)
		{
			n = Load(r, this, t);
			r.ReadNext();

			if(n == null)
			{
				break;
			}
		}

		//if(n == null || r.Current != EXonElement::End)
		//{
		//	Clear();
		//}
	}

	protected Xon Load(IXonReader r, Xon parent, Xon tparent)
	{
		//CString name;
		//CString type;
		Xon n = null;
		Xon t = null;

		while(r.Current != XonToken.End)
		{
			switch(r.Current)
			{
				case XonToken.NodeBegin:
					n = new Xon(Serializator);
					n.Parent = parent;
					break;

				case XonToken.NodeEnd:
					if(t != null)
					{
						if(tparent != null)
						{
							var d = tparent.Nodes.FirstOrDefault(j => j.Name == n.Name && j.Value != null && n.Value != null && n.Value.Equals(j.Value) ); // from default list?
							if(d != null)
								t = d;
						}
					
						foreach(var i in t.Nodes)
						{
							if(!i.IsTemplate)
							{
								var c = n.Nodes.FirstOrDefault(j => j.Name == i.Name);
								if(c == null)
								{
									n.Nodes.Add(i.CloneInternal(this));
								}
							}
						}
					}
					return n;

				case XonToken.NameBegin:
				{
					n.Name = r.ParseName();

					var pre = n.Name[0];

					if(pre == '-')
					{
						n.Name = n.Name.Substring(1);
						n.IsRemoved = true;
					}
					if(pre == '*')
					{
						n.Name = n.Name.Substring(1);
					}

					if(tparent != null)
					{
						t = tparent.Templates.FirstOrDefault(i => i.Name == n.Name); // multi merge
						if(t == null)
							t = (Xon)tparent.Nodes.FirstOrDefault(i => i.Name == n.Name); // single merge
					}
					else if(parent != null) // self merge
					{
						t = parent.Templates.FirstOrDefault(i => i.Name == n.Name);
					}
				
					if(t != null)
					{
						parent.Nodes.Add(n);
					}
					else
					{
						if(pre == '*') // this is template
						{
							parent.IsTemplate = true;
							parent.Templates.Add(n);
						}
						else
							parent.Nodes.Add(n); // to children
					}

					if(t != null && t.Value != null)
						n.Value = t.Value;
					//else
					//	if(StandardTypes.ContainsKey(n.Type))
					//		n.Value = StandardTypes[n.Type].Clone();
					//	else if(Types.ContainsKey(n.Type))
					//	{
					//		n.Value = Types[n.Type].Clone();
					//		n.Type = Types[n.Type].GetTypeName();
					//	}
					break;
				}	

				case XonToken.MetaBegin:
					n.Meta = r.ParseMeta();
					break;

				case XonToken.SimpleValueBegin:
					n._Value = r.ParseValue();
					break;

				case XonToken.AttrValueBegin:
				{
					var a = new Xon(Serializator);
					//a.Name = "";
					n.Value = a;
	
					while(r.ReadNext() == XonToken.NodeBegin)
					{
						Load(r, a, null);

						//if(r.Current == XonToken.AttrValueEnd)
						//{
						//	break;;
						//}
					}
					break;
				}

				case XonToken.ChildrenBegin:
					while(r.ReadNext() == XonToken.NodeBegin)
					{
						if(Load(r, n, t) == null)
						{
							return n;
						}
					}
					//while(var i = p.Children.Find([](var j){ return j.IsRemoved; }))
					//{
					//	p.Children.remove(i);
					//	p.Removed.Add(i);
					//}
					break;
			}
		
			r.ReadNext();
		}
		return n;
	}

	public void Save(IXonWriter w)
	{
		w.Start();
		w.Write(this);
		w.Finish();
	}

	public void Save(string path)
	{
		using(var s = File.Create(path))
		{
			Save(new XonTextWriter(s, Encoding.UTF8));
		}
	}
	
	public void Dump(Action<Xon, int> write)
	{
		void dump(Xon n, int l)
		{
			write(n, l);

			foreach(var i in n.Nodes)
			{
				dump(i, l + 1);
			}
		}

		foreach(var n in Nodes)
			dump(n, 0);
	}
}

public class XonJsonConverter : JsonConverter<Xon>
{
	public override Xon Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return new Xon(reader.GetString());
	}

	public override void Write(Utf8JsonWriter writer, Xon value, JsonSerializerOptions options)
	{
		var s = new MemoryStream();
		value.Save(new XonTextWriter(s, Encoding.Default));
		
		writer.WriteStringValue(Encoding.Default.GetString(s.ToArray()));
	}

}
