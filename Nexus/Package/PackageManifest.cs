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
	public AutoId			Id { get; set; }
	public Ura				Address { get; set; }
	public DependencyNeed	Need { get; set; }
	public DependencyFlag	Flags { get; set; }

	public override string ToString()
	{
		return $"{Address}, {Need}, {Flags}";
	}

	public void Read(Reader reader)
	{
		Id		= reader.Read<AutoId>();
		Need	= reader.Read<DependencyNeed>();
		Flags	= reader.Read<DependencyFlag>();
	}
	
	public void Write(Writer writer)
	{
		writer.Write(Id);
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
		
		d.Address	= Ura.Parse(xon.Name);
		d.Id		= xon.Parse(v => AutoId.Parse(v));
		d.Need		= Enum.Parse<DependencyNeed>(xon.Get<string>("Need"));
		d.Flags		|= xon.Has(DependencyFlag.Merge.ToString()) ? DependencyFlag.Merge : DependencyFlag.None;
		d.Flags		|= xon.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.None;

		return d;
	}

	internal Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Name = Address.ToString();
		x.Value = Id?.ToString();
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
	public AutoId				Id { get; set; }
	public Ura					Address { get; set; }
	public ParentPackageFlag	Flags { get; set; }
	public Dependency[]			AddedDependencies { get; set; } = [];
	public Dependency[]			RemovedDependencies { get; set; } = [];

	public void Write(Writer w)
	{
		w.Write(Id);
		w.Write(Flags);
		w.Write(AddedDependencies);
		w.Write(RemovedDependencies);
	}
	
	public void Read(Reader r)
	{						
		Id					= r.Read<AutoId>();
		Flags				= r.Read<ParentPackageFlag>();
		AddedDependencies	= r.ReadArray<Dependency>();
		RemovedDependencies = r.ReadArray<Dependency>();
	}

	public static ParentPackage FromXon(Xon xon)
	{
		var d = new ParentPackage();

		d.Address				= Ura.Parse(xon.Name);
		d.Id					= xon.Parse(v => AutoId.Parse(v));
		d.Flags					= xon.GetEnum<ParentPackageFlag>("Flags", ParentPackageFlag.None);
		d.AddedDependencies		= [..xon.One("Add")?.Nodes.Select(Dependency.FromXon) ?? []];
		d.RemovedDependencies	= [..xon.One("Remove")?.Nodes.Select(Dependency.FromXon) ?? []];

		return d;
	}
	
	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Name = Address.ToString();
		x.Value = Id?.ToString();
		x.Add("Flags").Value = Flags;

		if(AddedDependencies.Any())
			x.Add("Add").Nodes.AddRange(AddedDependencies.Select(i => i.ToXon(serializator)));

		if(RemovedDependencies.Any())
			x.Add("Remove").Nodes.AddRange(RemovedDependencies.Select(i => i.ToXon(serializator)));

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
	public const string				Extension = ".rdnpm";

	public Urn						Urn { get; set; }
	public Dependency[]				CompleteDependencies { get; set; } = [];
	public ParentPackage[]			Parents { get; set; } = [];

	[JsonIgnore]
	public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical);

	public PackageManifest()
	{
  	}

	public PackageManifest(byte[] bytes)
	{
		Read(new Reader(bytes));
  	}

	public void Write(Writer writer)
	{
		writer.WriteVirtual(Urn); /// URN HEADER: MUST GO FIRST

		writer.Write(CompleteDependencies);
		writer.Write(Parents);
	}

	public void Read(Reader reader)
	{
		Urn						= reader.ReadVirtual<Urn>(); /// URN HEADER: MUST GO FIRST

		CompleteDependencies	= reader.ReadArray<Dependency>();
		Parents					= reader.ReadArray<ParentPackage>();
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

		x.Add(nameof(Urn)).Value = Urn?.ToString();
		
		if(CompleteDependencies.Any())
			x.Add(nameof(CompleteDependencies)).Nodes.AddRange(CompleteDependencies.Select(i => i.ToXon(serializator)));

		if(Parents.Any())
			x.Add(nameof(Parents)).Nodes.AddRange(Parents.Select(i => i.ToXon(serializator)));

		return x;
	}

	public static PackageManifest FromXon(Xon xon)
	{
		var m = new PackageManifest();

		m.Urn					= xon.One(nameof(Urn))?.Parse(v => Urn.Parse(v));
		m.CompleteDependencies	= xon.One(nameof(CompleteDependencies))?.Nodes.Select(Dependency.FromXon).ToArray() ?? [];
		m.Parents				= xon.One(nameof(Parents))?.Nodes.Select(ParentPackage.FromXon).ToArray() ?? [];

		return m;
	}

	public void TranslateAddressToId(Func<Ura, AutoId> getid)
	{
		foreach(var c in CompleteDependencies)
		{
			c.Id = getid(c.Address);
		}

		foreach(var p in Parents)
		{
			p.Id = getid(p.Address);

			foreach(var a in p.AddedDependencies)
			{
				a.Id = getid(a.Address);
			}

			foreach(var r in p.RemovedDependencies)
			{
				r.Id = getid(r.Address);
			}
		}
	}
}
