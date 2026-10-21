using System.Text;
using System.Text.Json.Serialization;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public enum DependencyNeed : byte
{
	None, Critical, Deferred
}

[Flags]
public enum DependencyFlag : byte
{
	None, 
	Merge				= 0b0000_0001, 
	AutoUpdateAllowed	= 0b0000_0010,
}

public class Dependency : IEquatable<Dependency>, IBinarySerializable
{
	public Ura				Address { get; set; }
	public DependencyNeed	Need { get; set; }
	public DependencyFlag	Flags { get; set; }

	public Dependency()
	{
	}

	public Dependency(Ura address, DependencyNeed need = DependencyNeed.None, DependencyFlag flags = DependencyFlag.None)
	{
		Address = address;
		Need = need;
		Flags = flags;
	}

	public override string ToString()
	{
		return $"{Address}, {Need}, {Flags}";
	}

	public void Read(Reader reader)
	{
		Address			= reader.Read<Ura>();
		Need			= reader.Read<DependencyNeed>();
		Flags			= reader.Read<DependencyFlag>();
	}
	
	public void Write(Writer writer)
	{
		writer.Write(Address);
		writer.Write(Need);
		writer.Write(Flags);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Dependency);
	}

	public bool Equals(Dependency other)
	{
		return other is not null && Need == other.Need && Flags == other.Flags && Address == other.Address;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Need, Flags, Address);
	}

	public static bool operator ==(Dependency left, Dependency right)
	{
		return EqualityComparer<Dependency>.Default.Equals(left, right);
	}

	public static bool operator !=(Dependency left, Dependency right)
	{
		return !(left == right);
	}

	public static Dependency FromXon(Xon xon)
	{
		var d = new Dependency();
		
		d.Address		= Ura.Parse(xon.Name);
		d.Need			= Enum.Parse<DependencyNeed>(xon.Get<string>("Need"));
		d.Flags			|= xon.Has(DependencyFlag.Merge.ToString()) ? DependencyFlag.Merge : DependencyFlag.None;
		d.Flags			|= xon.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.None;

		return d;
	}

	internal Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Name = Address.ToString();
// 
// 		if(Dependencies.Any())
// 		{
// 			var d = x.Add(nameof(Dependencies));
// 			
// 			foreach(var i in Dependencies)
// 			{
// 				d.Nodes.Add(i.ToXon(serializator));
// 			}
// 		}

		x.Add("Need").Value = Need;
		
		foreach(var i in Enum.GetValues<DependencyFlag>().Where(i => i != DependencyFlag.None))
		{
			if(Flags.HasFlag(i))
				x.Add(i.ToString());
		}

		return x;
	}
}	

[Flags]
public enum ParentPackageFlag : byte
{
	None, 
	Software_Compatible	= 0b0000_0001,
}

public class ParentPackage : IBinarySerializable
{
	public Ura					Address { get; set; }
	public ParentPackageFlag	Flags { get; set; }

	public ParentPackage()
	{
	}

	public ParentPackage(Ura address, ParentPackageFlag flags = ParentPackageFlag.None)
	{
		Address = address;
		Flags = flags;
	}

	public void Write(Writer w)
	{
		w.Write(Address);
		w.Write(Flags);
	}
	
	public void Read(Reader r)
	{						
		Address				= r.Read<Ura>();
		Flags				= r.Read<ParentPackageFlag>();
	}

	public static ParentPackage FromXon(Xon xon)
	{
		var d = new ParentPackage();

		d.Address				= Ura.Parse(xon.Name);
		d.Flags					= xon.GetEnum<ParentPackageFlag>("Flags", ParentPackageFlag.None);

		return d;
	}
	
	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Name = Address.ToString();
		x.Add("Flags").Value = Flags;

		return x;
	}
}

[Flags]
public enum PackageFlag : byte
{
	None, 
	Deprecated		= 0b0000_0001, 
}

public class PackageManifest : IBinarySerializable
{
	public const string				Extension = "rdnpm";

	public Dependency[]				Dependencies { get; set; } = []; /// All dependencies and its dependencies recursively
	public ParentPackage[]			Parents { get; set; } = [];

	[JsonIgnore]
	public IEnumerable<Dependency>	Criticals => Dependencies.Where(i => i.Need == DependencyNeed.Critical);

	public PackageManifest()
	{
  	}

	public PackageManifest(byte[] bytes)
	{
		Read(new Reader(bytes));
  	}

	public void Write(Writer writer)
	{
		writer.Write(Dependencies);
		writer.Write(Parents);
	}

	public void Read(Reader reader)
	{
		Dependencies	= reader.ReadArray<Dependency>();
		Parents			= reader.ReadArray<ParentPackage>();
	}

	public static PackageManifest Parse(string text)
	{
		return FromXon(new Xon(text));
	}

	public static PackageManifest Load(string filepath)
	{
		return FromXon(new Xon(File.ReadAllText(filepath, Encoding.UTF8)));
	}

	public void Save(string filepath)
	{
		ToXon().Save(filepath);
	}

	public Xon ToXon(IXonValueSerializator serializator = null)
	{
		serializator ??= new XonTextValueSerializator();

		var x = new Xon(serializator);
		
		if(Dependencies.Any())
			x.Add(nameof(Dependencies)).Nodes.AddRange(Dependencies.Select(i => i.ToXon(serializator)));

		if(Parents.Any())
			x.Add(nameof(Parents)).Nodes.AddRange(Parents.Select(i => i.ToXon(serializator)));

		return x;
	}

	public static PackageManifest FromXon(Xon xon)
	{
		var m = new PackageManifest();

		m.Dependencies	= xon.One(nameof(Dependencies))?.Nodes.Select(Dependency.FromXon).ToArray() ?? [];
		m.Parents		= xon.One(nameof(Parents))?.Nodes.Select(ParentPackage.FromXon).ToArray() ?? [];

		return m;
	}

// 	public void TranslateAddressToId(Func<Ura, AutoId> getid)
// 	{
// 		foreach(var c in CompleteDependencies)
// 		{
// 			c.Id = getid(c.Address);
// 		}
// 
// 		foreach(var p in Parents)
// 		{
// 			p.Id = getid(p.Address);
// 
// 			foreach(var a in p.AddedDependencies)
// 			{
// 				a.Id = getid(a.Address);
// 			}
// 
// 			foreach(var r in p.RemovedDependencies)
// 			{
// 				r.Id = getid(r.Address);
// 			}
// 		}
// 	}
}
