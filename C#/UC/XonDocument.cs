using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC
{
	public class XonDocument : Xon
	{
		static XonDocument()
		{
		}
		
		public XonDocument(IXonValueSerializator serializator) : base(serializator)
		{
		}
		
		public XonDocument(XonTextReader r) : base(new TextXonValueSerializator())
		{
			Load(null, r);
		}

		internal void Load(XonDocument t, XonTextReader r)
		{
			r.Read();
	
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

		protected Xon Load(XonTextReader r, Xon parent, Xon tparent)
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
						r.ParseName(ref n.Name, ref n.Type);

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

					case XonToken.SimpleValueBegin:
						n.Value = r.ParseValue();
						break;

					case XonToken.AttrValueBegin:
					{
						var a = new Xon(Serializator);
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
			w.WriteHeader();

			foreach(var i in Nodes)
			{
				w.Write(i);
			}

			w.Finish();
		}
	}
}
