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
		d.Need		= Enum.Parse<DependencyNeed>(xon.Get<string>("Need"));
		d.Flags		|= xon.Has(DependencyFlag.Merge.ToString()) ? DependencyFlag.Merge : DependencyFlag.None;
		d.Flags		|= xon.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.None;

		return d;
	}

	internal Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);

		x.Name = Address.ToString();
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
	public Dependency[]			AddedDependencies { get; set; }
	public Dependency[]			RemovedDependencies { get; set; }

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
		d.Flags					= xon.GetEnum<ParentPackageFlag>("Flags", ParentPackageFlag.None);
		d.AddedDependencies		= xon.One("Add").Nodes.Select(Dependency.FromXon).ToArray();
		d.RemovedDependencies	= xon.One("Remove").Nodes.Select(Dependency.FromXon).ToArray();

		return d;
	}
	
	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Name = Address.ToString();
		x.Add("Flags").Value = Flags;
		x.Add("Add").Nodes.AddRange(AddedDependencies.Select(i => i.ToXon(serializator)));
		x.Add("Remove").Nodes.AddRange(RemovedDependencies.Select(i => i.ToXon(serializator)));

		return x;
	}
}

public class Start : IBinarySerializable
{
	public string		Path { get; set; }
	public string		Arguments { get; set; }
	public Expression	Condition { get; set; }

	public override string ToString()
	{
		return $"{Path}, {Arguments}";
	}

	public void Read(Reader reader)
	{
		Path		= reader.ReadUtf8();
		Arguments	= reader.ReadUtf8();
		Condition	= reader.ReadNullable<Expression>();
	}
	
	public void Write(Writer writer)
	{
		writer.WriteUtf8(Path);
		writer.WriteUtf8(Arguments);
		writer.WriteNullable(Condition);
	}

	public static Start FromXon(Xon x)
	{
		var m = new Start ();
		m.Path		= x.Get<string>("Path", null);
		m.Arguments	= x.Get<string>("Arguments", null);
		m.Condition	= x.Has("Condition") ? Expression.FromXon(x.One("Condition").Nodes.First()) : null;

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Add("Path").Value = Path;

		if(Arguments != null)	x.Add(nameof(Arguments)).Value = Arguments;
		if(Condition != null)	x.Add(nameof(Condition)).Nodes.Add(Condition.ToXon(serializator));

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

	public byte[]					CompleteHash { get; set; }
	public Dependency[]				CompleteDependencies { get; set; } = [];
	public byte[]					IncrementalHash { get; set; }
	public ParentPackage[]			Parents { get; set; }
	public Start[]					Start { get; set; }

	[JsonIgnore]
	public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical);

	public Start					MatchExecution(Platform platform) => Start.FirstOrDefault(i => i.Condition.Match(platform)); 

  	public byte[] Raw
  	{
  		get
  		{
 	 		var s = new MemoryStream();
 	 	
			ToXon(new NetXonTextValueSerializator()).Save(new XonTextWriter(s, Encoding.UTF8));
	
 	 		return s.ToArray();
  		}
  	}

	public PackageManifest()
	{
  	}

	public void Write(Writer writer)
	{
		writer.Write(CompleteHash);
		writer.Write(CompleteDependencies);
		writer.Write(IncrementalHash);
		writer.Write(Parents);
		writer.Write(Start);
	}

	public void Read(Reader reader)
	{
		CompleteHash			= reader.ReadHash();
		CompleteDependencies	= reader.ReadArray<Dependency>();
		IncrementalHash			= reader.ReadHash();
		Parents					= reader.ReadArray<ParentPackage>();
		Start					= reader.ReadArray<Start>();
	}

	public Start MatchExecution(Family family)
	{ 
		var p = new Platform {Family = family};
		return Start.FirstOrDefault(i => i.Condition.Match(p)); 
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
		ToXon(new NetXonTextValueSerializator()).Save(filepath);
	}

	public static PackageManifest FromXon(Xon xon)
	{
		var m = new PackageManifest();

		m.CompleteHash			= xon.Get<byte[]>("Complete/Hash");
		m.CompleteDependencies	= xon.One("Complete/Dependencies")?.Nodes.Select(Dependency.FromXon).ToArray() ?? [];
		m.IncrementalHash		= xon.Get<byte[]>("Incremental/Hash", null);
		m.Parents				= xon.One("Incremental/Parents")?.Nodes.Select(ParentPackage.FromXon).ToArray();
		m.Start					= xon.Many(nameof(Start)).Select(Uccs.Nexus.Start.FromXon).ToArray();

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{
		var x = new Xon(serializator);
		
		var c = x.Add("Complete");
		c.Add("Hash").Value = CompleteHash;
		
		if(CompleteDependencies.Any())
			c.Add("Dependencies").Nodes.AddRange(CompleteDependencies.Select(i => i.ToXon(serializator)));

		if(IncrementalHash != null)
		{
			var i = x.Add("Incremental");
			i.Add("Hash").Value = IncrementalHash;
		
			if(Parents != null && Parents.Any())
				i.Add("Parents").Nodes.AddRange(Parents.Select(i => i.ToXon(serializator)));
		}

		if(Start != null && Start.Any())
			x.Nodes.AddRange(Start.Select(i => {
													var e = i.ToXon(serializator);
													e.Name = nameof(Start);
													return e;
												}));

		return x;
	}
}

public class ChangableExtra : IBinarySerializable
{
	public PackageFlag		Flags { get; set; }
	public Ura				Replacement { get; set; }

	public void Read(Reader reader)
	{
		Flags		= reader.Read<PackageFlag>();
		Replacement = reader.Read<Ura>();
	}

	public void Write(Writer writer)
	{
		writer.Write(Flags);
		writer.Write(Replacement);
	}
}