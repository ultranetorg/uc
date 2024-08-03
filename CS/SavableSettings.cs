using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs
{
	public class Settings
	{
		protected IXonValueSerializator			Serializator;
		
		public Settings(IXonValueSerializator serializator)
		{
			Serializator  = serializator;
		}

 		protected object Load(string name, Type t, Xon x)
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
					a.GetMethod("Set").Invoke(l, [i, Load(name.TrimEnd('s'), t.GetElementType(), m[i])]);
				}

				return l;
			}
			else
				return x.Get(t);
 		}

		public void Load(Xon xon)
		{
			foreach(var p in GetType().GetProperties().Where(i => i.CanRead && i.CanWrite && i.SetMethod.IsPublic))
			{
				var x = xon.One(p.Name) ?? xon.One(p.Name.TrimEnd('s'));
		
				if(p.PropertyType == typeof(bool))	
				{
					p.SetValue(this, x != null);
				}
				else if(x != null)
				{
					if(p.PropertyType.Name.EndsWith("Settings"))
					{
						var s = Activator.CreateInstance(p.PropertyType) as Settings;
						s.Load(x);
						p.SetValue(this, s);
					}
					else
						p.SetValue(this, Load(p.Name, p.PropertyType, x));
				}
			}
		}
	}

	public class SavableSettings : Settings
	{
		public string					Profile;
		public virtual string			FileName => GetType().Name.Remove(GetType().Name.Length - nameof(Settings).Length) + Extention;
		public string					Path => System.IO.Path.Join(Profile, FileName); 
		public const string				Extention = ".settings";
		
		public SavableSettings(IXonValueSerializator serializator) : base(serializator)
		{
		}
		
		public SavableSettings(string profile, IXonValueSerializator serializator) : this(serializator)
		{
			Directory.CreateDirectory(profile);

			Profile = profile;

			if(File.Exists(Path))
			{
				var x = new Xon(File.ReadAllText(Path), serializator);
	
				Load(x);
			}
		}

		public SavableSettings Merge(Xon x)
		{
			var r = Activator.CreateInstance(GetType()) as SavableSettings;

			r.Profile = Profile;

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
						if(p.GetValue(r) is SavableSettings s)
							s.Merge(v);
						else
						{
							s = Activator.CreateInstance(p.PropertyType) as SavableSettings;
							s.Load(v);
							p.SetValue(r, s);
						}
					}
					else
						p.SetValue(r, Load(p.Name, p.PropertyType, v));
				} 
				else
					p.SetValue(r, p.GetValue(this));
			}

			return r;
		}
		
		public Xon Save()
		{
			var doc = new Xon(Serializator);

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
					else if(type.IsArray && value != null)
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

			Directory.CreateDirectory(Profile);

			using(var s = File.Create(Path))
			{
				doc.Save(new XonTextWriter(s, Encoding.UTF8));
			}

			return doc;
		}
	}
}
