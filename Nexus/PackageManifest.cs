using System.Text;
using System.Text.Json.Serialization;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public enum DependencyNeed
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

public class Dependency : IEquatable<Dependency>
{
	public Ura				Address { get; set; }
	public DependencyNeed	Need { get; set; }
	public DependencyFlag	Flags { get; set; }

	public override string ToString()
	{
		return $"{Address}, {Need}, {Flags}";
	}

	//public void Read(BinaryReader reader)
	//{
	//	Package = reader.Read<Ura>();
	//	Type = (DependencyType)reader.ReadByte();
	//	Flags = (DependencyFlag)reader.ReadByte();
	//}
	//
	//public void Write(BinaryWriter writer)
	//{
	//	writer.Write(Package);
	//	writer.Write((byte)Type);
	//	writer.Write((byte)Flags);
	//}

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

public class ParentPackage
{
	public Ura					Release { get; set; }
	public ParentPackageFlag	Flags { get; set; }
	public Dependency[]			AddedDependencies { get; set; }
	public Dependency[]			RemovedDependencies { get; set; }

	public static ParentPackage FromXon(Xon xon)
	{
		var d = new ParentPackage();

		d.Release				= Ura.Parse(xon.Name);
		d.Flags					= xon.GetEnum<ParentPackageFlag>("Flags", ParentPackageFlag.None);
		d.AddedDependencies		= xon.One("Add").Nodes.Select(Dependency.FromXon).ToArray();
		d.RemovedDependencies	= xon.One("Remove").Nodes.Select(Dependency.FromXon).ToArray();

		return d;
	}
	
	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Name = Release.ToString();
		x.Add("Flags").Value = Flags;
		x.Add("Add").Nodes.AddRange(AddedDependencies.Select(i => i.ToXon(serializator)));
		x.Add("Remove").Nodes.AddRange(RemovedDependencies.Select(i => i.ToXon(serializator)));

		return x;
	}

	//public void Write(BinaryWriter w)
	//{
	//	w.Write(Release);
	//	w.Write(AddedDependencies);
	//	w.Write(RemovedDependencies);
	//}
	//
	//public void Read(BinaryReader r)
	//{						
	//	Release				= r.Read<Ura>();
	//	AddedDependencies	= r.ReadArray<Dependency>();
	//	RemovedDependencies = r.ReadArray<Dependency>();
	//}
}

public class Start// : IBinarySerializable
{
	public string				Path { get; set; }
	public string				Arguments { get; set; }
	public PlatformExpression	Condition { get; set; }

	public override string ToString()
	{
		return $"{Path}, {Arguments}";
	}

	//public void Read(BinaryReader reader)
	//{
	//	Path = reader.ReadUtf8();
	//	Arguments = reader.ReadUtf8();
	//}
	//
	//public void Write(BinaryWriter writer)
	//{
	//	writer.WriteUtf8(Path);
	//	writer.WriteUtf8(Arguments);
	//}

	public static Start FromXon(Xon x)
	{
		var m = new Start ();
		m.Path		= x.Get<string>("Path", null);
		m.Arguments	= x.Get<string>("Arguments", null);
		m.Condition	= x.Has("Condition") ? PlatformExpression.FromXon(x.One("Condition").Nodes.First()) : new PlatformExpression();

		return m;
	}

	public Xon ToXon(IXonValueSerializator serializator)
	{					
		var x = new Xon(serializator);
	
		x.Add("Path").Value = Path;
		x.Add("Arguments").Value = Arguments;
		x.Add("Condition").Nodes.Add(Condition.ToXon(serializator));

		return x;
	}
}	

[Flags]
public enum PackageFlag : byte
{
	None, 
	Deprecated		= 0b0000_0001, 
}

public class PackageManifest
{
	public const string				Extension = "rdnpm";

	public byte[]					CompleteHash { get; set; }
	public Dependency[]				CompleteDependencies { get; set; }
	public byte[]					IncrementalHash { get; set; }
	public ParentPackage[]			Parents { get; set; }
	public Ura[]					History { get; set; }
	public Start[]					Starts { get; set; }

	public Start					MatchExecution(Platform platform) => Starts.FirstOrDefault(i => i.Condition.Match(platform)); 

	[JsonIgnore]
	public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Need == DependencyNeed.Critical);

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
		m.History				= xon.One("History")?.Nodes.Select(i => Ura.Parse(i.Name)).ToArray();
		m.Starts				= xon.Many("Execution").Select(Start.FromXon).ToArray();

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

		if(History != null && History.Any())
			x.Add("History").Nodes.AddRange(History.Select(i => new Xon(serializator, i.ToString())));

		if(Starts != null && Starts.Any())
			x.Nodes.AddRange(Starts.Select(i => {
													var e = i.ToXon(serializator);
													e.Name = "Execution";
													return e;
												}));

		return x;
	}
}

public class ChangableExtra : IBinarySerializable
{
	public PackageFlag		Flags { get; set; }
	public Ura				Replacement { get; set; }

	public void Read(BinaryReader reader)
	{
		Flags		= reader.Read<PackageFlag>();
		Replacement = reader.Read<Ura>();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Flags);
		writer.Write(Replacement);
	}
}