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
		public PackageAddress	Release { get; set; }
		public DependencyType	Type { get; set; }
		public DependencyFlag	Flags { get; set; }

		internal static Dependency From(Xon i)
		{
			var d = new Dependency();
			
			d.Release	= PackageAddress.Parse(i.String);
			d.Type		= Enum.Parse<DependencyType>(i.Name);
			d.Flags		|= i.Has(DependencyFlag.SideBySide.ToString()) ? DependencyFlag.SideBySide : DependencyFlag.None;
			d.Flags		|= i.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.None;

			return d;
		}

		public override string ToString()
		{
			return $"{Release}, {Type}, {Flags}";
		}

		public void Read(BinaryReader reader)
		{
			Release = reader.Read<PackageAddress>();
			Type = (DependencyType)reader.ReadByte();
			Flags = (DependencyFlag)reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
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
				   EqualityComparer<PackageAddress>.Default.Equals(Release, other.Release);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Flags, Release);
		}

		public static bool operator ==(Dependency left, Dependency right)
		{
			return EqualityComparer<Dependency>.Default.Equals(left, right);
		}

		public static bool operator !=(Dependency left, Dependency right)
		{
			return !(left == right);
		}
	}	

	public class Manifest : IBinarySerializable
	{
		public PackageAddress			Address { get; set; }
		public byte[]					CompleteHash { get; set; }
		public Dependency[]				CompleteDependencies { get; set; }

		public byte[]					IncrementalHash { get; set; }
		public Version					IncrementalMinimalVersion { get; set; }
		public Dependency[]				AddedDependencies { get; set; }
		public Dependency[]				RemovedDependencies { get; set; }

		[JsonIgnore]
		public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Type == DependencyType.Critical);

		public Zone						Zone;

 		public byte[] Bytes
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

		public Manifest(Zone zone)
		{
			Zone = zone;
		}

		public Manifest(Zone					zone,
						byte[]					completehash,
						long					completelength,
						IEnumerable<Dependency>	completecoredependencies,
						byte[]					incrementalhash,
						long					incrementallength,
						Version					incrementalminimalversion,
						IEnumerable<Dependency>	addedcoredependencies,
						IEnumerable<Dependency>	removedcoredependencies)
		{
			Zone						= zone;
			CompleteHash				= completehash;
			CompleteDependencies		= completecoredependencies?.ToArray() ?? new Dependency[]{};
			
			IncrementalHash				= incrementalhash;
			IncrementalMinimalVersion	= incrementalminimalversion;

			AddedDependencies			= addedcoredependencies?.ToArray() ?? new Dependency[]{};
			RemovedDependencies			= removedcoredependencies?.ToArray() ?? new Dependency[]{};
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			if(CompleteHash != null)
			{
				d.Add("CompleteHash").Value = CompleteHash;
	
				if(CompleteDependencies.Any())
				{
					var cd = d.Add("CompleteDependencies");
	
					foreach(var i in CompleteDependencies)
					{
						cd.Add(i.ToString());
					}
				}
			}
	
			if(IncrementalHash != null)
			{
				d.Add("IncrementalHash").Value = IncrementalHash;
				d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;

				if(AddedDependencies.Any())
				{
					var cd = d.Add("AddedDependencies");

					foreach(var i in AddedDependencies)
					{
						cd.Add(i.ToString());
					}
				}
	
				if(RemovedDependencies.Any())
				{
					var cd = d.Add("RemovedDependencies");

					foreach(var i in RemovedDependencies)
					{
						cd.Add(i.ToString());
					}
				}
			}

			return d;		
		}

		public void Write(BinaryWriter w)
		{
			w.Write(CompleteHash != null);

			if(CompleteHash != null)
			{
				w.Write(CompleteHash);
				w.Write(CompleteDependencies);
			}
							
			w.Write(IncrementalHash != null);
				
			if(IncrementalHash != null)
			{
				w.Write(IncrementalHash);
				w.Write(IncrementalMinimalVersion);

				w.Write(AddedDependencies);
				w.Write(RemovedDependencies);
			}
		}

		public void Read(BinaryReader r)
		{
			if(r.ReadBoolean())
			{
				CompleteHash = r.ReadSha3();
				CompleteDependencies = r.ReadArray<Dependency>();
			}

			if(r.ReadBoolean())
			{
				IncrementalHash = r.ReadSha3();
				IncrementalMinimalVersion = r.Read<Version>();
				AddedDependencies = r.ReadArray<Dependency>();
				RemovedDependencies = r.ReadArray<Dependency>();
			}
		}
	}
}
