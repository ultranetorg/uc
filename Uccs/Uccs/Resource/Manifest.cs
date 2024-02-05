using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Uccs.Net
{
	public enum DependencyType
	{
		None, Critical, Deferred
	}

	[Flags]
	public enum DependencyFlag : byte
	{
		None, 
		SideBySide			= 0b0000_0001, 
		AutoUpdateAllowed	= 0b0000_0010
	}

	public class Dependency : IBinarySerializable, IEquatable<Dependency>
	{
		public PackageAddress	Package { get; set; }
		public DependencyType	Type { get; set; }
		public DependencyFlag	Flags { get; set; }

		public static Dependency FromXon(Xon i)
		{
			var d = new Dependency();
			
			d.Package	= PackageAddress.Parse(i.Name);
			d.Type		= Enum.Parse<DependencyType>(i.Get<string>("Type"));
			d.Flags		|= i.Has(DependencyFlag.SideBySide.ToString()) ? DependencyFlag.SideBySide : DependencyFlag.None;
			d.Flags		|= i.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.None;

			return d;
		}

		public override string ToString()
		{
			return $"{Package}, {Type}, {Flags}";
		}

		public void Read(BinaryReader reader)
		{
			Package = reader.Read<PackageAddress>();
			Type = (DependencyType)reader.ReadByte();
			Flags = (DependencyFlag)reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Package);
			writer.Write((byte)Type);
			writer.Write((byte)Flags);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Dependency);
		}

		public bool Equals(Dependency other)
		{
			return other is not null &&
				   Type == other.Type &&
				   Flags == other.Flags &&
				   Package == other.Package;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Flags, Package);
		}

		public static bool operator ==(Dependency left, Dependency right)
		{
			return EqualityComparer<Dependency>.Default.Equals(left, right);
		}

		public static bool operator !=(Dependency left, Dependency right)
		{
			return !(left == right);
		}

		internal Xon ToXon(IXonValueSerializator serializator)
		{
			var x = new Xon(serializator);

			x.Name = Package.ToString();
			x.Add("Type").Value = Type;
			
			foreach(var i in Enum.GetValues<DependencyFlag>().Where(i => i != DependencyFlag.None))
			{
				if(Flags.HasFlag(i))
					x.Add(i.ToString());
			}

			return x;
		}
	}	

	public class ParentPackage : IBinarySerializable
	{
		public ReleaseAddress	Release { get; set; }
		public Dependency[]		AddedDependencies { get; set; }
		public Dependency[]		RemovedDependencies { get; set; }

		public void Write(BinaryWriter w)
		{
			w.Write(Release);
			w.Write(AddedDependencies);
			w.Write(RemovedDependencies);
		}

		public void Read(BinaryReader r)
		{
			Release				= ReleaseAddress.FromRaw(r);
			AddedDependencies	= r.ReadArray<Dependency>();
			RemovedDependencies = r.ReadArray<Dependency>();
		}
	}

	public class Manifest : IBinarySerializable
	{
		public byte[]					CompleteHash { get; set; }
		public Dependency[]				CompleteDependencies { get; set; }
		public byte[]					IncrementalHash { get; set; }
		public ParentPackage[]			Parents { get; set; }
		public ReleaseAddress[]			History { get; set; }

		public const string				Extension = "manifest";

		[JsonIgnore]
		public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Type == DependencyType.Critical);

 		public byte[] Raw
 		{
 			get
 			{
	 			var s = new MemoryStream();
	 			var w = new BinaryWriter(s);
	 	
				Write(w);
	 		
	 			return s.ToArray();
 			}
 		}

		public Manifest()
		{
		}

		public static Manifest LoadCompleteDependencies(string filepath)
		{
			var d = new XonDocument(File.ReadAllText(filepath));

			var m = new Manifest();
			m.CompleteDependencies = d.One("Complete/Dependencies").Nodes.Select(i => Dependency.FromXon(i)).ToArray();

			return m;
		}

		public void SaveCompleteDependencies(string filepath)
		{
			var d = new XonDocument(XonTextValueSerializator.Default);

			var s = d.Add("Complete");

			var deps = s.Add("Dependencies");

			foreach(var i in CompleteDependencies)
			{
				deps.Nodes.Add(i.ToXon(d.Serializator));
			}

			d.Save(filepath);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(History, i => {
											writer.Write(i.TypeCode); 
											i.Write(writer); 
									  });
			writer.WriteBytes(CompleteHash);
			writer.WriteBytes(IncrementalHash);
			writer.Write(CompleteDependencies);
			writer.Write(Parents);
		}

		public void Read(BinaryReader reader)
		{
			History					= reader.ReadArray(() => {
 																var o = ReleaseAddress.FromType(reader.ReadByte());
 																o.Read(reader); 
 																return o; 
 															});
			CompleteHash			= reader.ReadBytes();
			IncrementalHash			= reader.ReadBytes();
			CompleteDependencies	= reader.ReadArray<Dependency>();
			Parents					= reader.ReadArray<ParentPackage>();
		}

// 		public void Save(string filepath)
// 		{
// 			ToXon(XonTextValueSerializator.Default).Save(filepath);
// 		}
// 
// 		public XonDocument ToXon(IXonValueSerializator serializator)
// 		{
// 			var d = new XonDocument(serializator);
// 
// 			if(CompleteHash != null)
// 			{
// 				var s = d.Add("Complete");
// 
// 				s.Add("Hash").Value = CompleteHash;
// 
// 				if(CompleteDependencies.Any())
// 				{
// 					var deps = s.Add("Dependencies");
// 
// 					foreach(var i in CompleteDependencies)
// 					{
// 						deps.Nodes.Add(i.ToXon(serializator));
// 					}
// 				}
// 			}
// 	
// 			if(IncrementalHash != null)
// 			{
// 				var s = d.Add("Incremental");
// 
// 				s.Add("Hash").Value = IncrementalHash;
// 
// 				if(AddedDependencies.Any())
// 				{
// 					var a = s.Add("AddedDependencies");
// 
// 					foreach(var i in AddedDependencies)
// 					{
// 						a.Nodes.Add(i.ToXon(serializator));
// 					}
// 				}
// 	
// 				if(RemovedDependencies.Any())
// 				{
// 					var r = s.Add("RemovedDependencies");
// 
// 					foreach(var i in RemovedDependencies)
// 					{
// 						r.Nodes.Add(i.ToXon(serializator));
// 					}
// 				}
// 			}
// 
// 			return d;		
// 		}
	}
}
