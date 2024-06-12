using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uccs
{
	public class Settings
	{
		public string					Profile;
		public string					Path; 
		IXonValueSerializator			Serializator;
		
		public Settings(IXonValueSerializator serializator)
		{
			Serializator  = serializator;
		}
		
		public Settings(string profile, string filename, IXonValueSerializator serializator) : this(serializator)
		{
			Directory.CreateDirectory(profile);

			Profile = profile;
			Path = System.IO.Path.Join(profile, filename);

			if(File.Exists(Path))
			{
				var x = new XonDocument(File.ReadAllText(Path), serializator);
	
				Load(x);
			}
		}

 		object load(string name, Type t, Xon x)
 		{
			if(t.IsArray)
			{
				var m = x.Parent.Many(name.TrimEnd('s'));
				
				//.Select(i => load(n, t.GetElementType(), i)).ToArray();


				var a = t.GetElementType().MakeArrayType(1);
				var n = m.Count;
				var l = a.GetConstructor([typeof(int)]).Invoke([n]);
	
				for(int i=0; i<n; i++)
				{
					a.GetMethod("Set").Invoke(l, [i, load(name.TrimEnd('s'), t.GetElementType(), m[i])]);
				}

				return l;
			}
			else
				return x.Get(t);
 		}

		public void Load(Xon x)
		{
			foreach(var i in x.Nodes.GroupBy(i => i.Name))
			{
				var p = GetType().GetProperty(i.Key) ?? GetType().GetProperty(i.Key + "s");

				if(p.PropertyType == typeof(bool))	
				{
					p.SetValue(this, true);
				}
				else if(p.PropertyType.Name.EndsWith("Settings"))
				{
					var s = Activator.CreateInstance(p.PropertyType) as Settings;
					s.Load(i.First());
					p.SetValue(this, s);
				}
				else
					p.SetValue(this, load(p.Name, p.PropertyType, i.First()));
			}
		}

		public Settings Merge(Xon x)
		{
			var r = Activator.CreateInstance(GetType()) as Settings;

			r.Profile = Profile;
			r.Path = Path;

			foreach(var p in GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				var v = x.One(p.Name);
				
				if(v != null)
				{
					if(p.PropertyType == typeof(bool))	
					{
						p.SetValue(r, true);
					}
					else if(p.PropertyType.Name.EndsWith("Settings"))
					{
						if(p.GetValue(r) is Settings s)
							s.Merge(v);
						else
						{
							s = Activator.CreateInstance(p.PropertyType) as Settings;
							s.Load(v);
							p.SetValue(r, s);
						}
					}
					else
						p.SetValue(r, load(p.Name, p.PropertyType, v));
				} 
				else
					p.SetValue(r, p.GetValue(this));
			}

			return r;
		}
		
		public XonDocument Save()
		{
			var doc = new XonDocument(Serializator);

			void save(Xon parent, string name, Type type, object value)
			{
				if(type.Name.EndsWith("Settings"))
				{
					if(value != null)
					{
						var x = parent.Add(name);
						///var v = fi.GetValue(owner);
	
						foreach(var f in type.GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
						{
							save(x, f.Name, f.PropertyType, f.GetValue(value));
						}
					}
				}
				else
					if(type == typeof(bool))	
					{ 
						if((bool)value)
						{
							parent.Add(name);
						}
					}
					else if(type.IsArray)
					{
						foreach(var i in value as IEnumerable)
						{
							save(parent, name.Trim('s'), type.GetElementType(), i);
						}
					}
					else
						parent.Add(name).Value = value;
			}

			foreach(var i in GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				if(i.Name == nameof(Profile))
					continue;

				save(doc, i.Name, i.PropertyType, i.GetValue(this));
			}

			using(var s = File.Create(Path))
			{
				doc.Save(new XonTextWriter(s, Encoding.UTF8));
			}

			return doc;
		}
	}
}
