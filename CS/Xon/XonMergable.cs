using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uccs;

public class XonMergable : Xon
{
	public List<Xon>				Templates = new List<Xon>();
	public bool						IsTemplate = false;
	public List<Xon>				Removed;
	public bool						IsRemoved = false;

	public bool						IsDifferenceDeleted { get => false; }

	public XonMergable()
	{
	}

	public XonMergable(IXonValueSerializator serializator)
	{
		Serializator = serializator;
	}

	public XonMergable(IXonValueSerializator serializator, string name) : this(serializator)
	{
		Parent		= null;
		Name		= name;
		IsTemplate	= false;
	}

	public XonMergable(string text) : this(XonTextValueSerializator.Default)
	{
		Load(null, new XonTextReader(text));
	}

	public XonMergable(string text, IXonValueSerializator serializator) : this(serializator)
	{
		Load(null, new XonTextReader(text));
	}

	public XonMergable(byte[] data) : this(XonBinaryValueSerializator.Default)
	{
		Load(null, new XonBinaryReader(new MemoryStream(data)));
	}
	
	public XonMergable(IXonReader r, IXonValueSerializator serializator) : this(serializator)
	{
		Load(null, r);
	}

	public Xon CloneInternal(Xon parent)
	{
		var p = new XonMergable(Serializator, Name);
		p.Parent = parent;

		foreach(var i in Nodes)
		{
			p.Nodes.Add((i as XonMergable).CloneInternal(p));
		}

		p.Templates = Templates;

		if(Value != null)
		{
			p.Value = Value;
		}
		return p;
	}


	internal void Load(XonMergable t, IXonReader r)
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

	protected Xon Load(IXonReader r, XonMergable parent, XonMergable tparent)
	{
		//CString name;
		//CString type;
		XonMergable n = null;
		XonMergable t = null;

		while(r.Current != XonToken.End)
		{
			switch(r.Current)
			{
				case XonToken.NodeBegin:
					n = new XonMergable(Serializator);
					n.Parent = parent;
					break;

				case XonToken.NodeEnd:
					if(t != null)
					{
						if(tparent != null)
						{
							var d = tparent.Nodes.FirstOrDefault(j => j.Name == n.Name && j.Value != null && n.Value != null && n.Value.Equals(j.Value) ); // from default list?
							if(d != null)
								t = d as XonMergable;
						}
					
						foreach(var i in t.Nodes.Cast<XonMergable>())
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
						t = tparent.Templates.FirstOrDefault(i => i.Name == n.Name) as XonMergable; // multi merge
						if(t == null)
							t = (Xon)tparent.Nodes.FirstOrDefault(i => i.Name == n.Name) as XonMergable; // single merge
					}
					else if(parent != null) // self merge
					{
						t = parent.Templates.FirstOrDefault(i => i.Name == n.Name) as XonMergable;
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
					var a = new XonMergable(Serializator);
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
