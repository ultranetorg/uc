using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public enum DependencyType
	{
		Null, Critical, Deferred
	}

	public enum DependencyFlag : byte
	{
		Null, 
		SideBySide			= 0b0000_0001, 
		AutoUpdateAllowed	= 0b0000_0010
	}

	public class Dependency : IBinarySerializable, IEquatable<Dependency>
	{
		public DependencyType Type;
		public DependencyFlag Flags;
		public ReleaseAddress Release;

		internal static Dependency From(Xon i)
		{
			var d = new Dependency();
			
			d.Release	= ReleaseAddress.Parse(i.String);
			d.Type		= Enum.Parse<DependencyType>(i.Name);
			d.Flags		|= i.Has(DependencyFlag.SideBySide.ToString()) ? DependencyFlag.SideBySide : DependencyFlag.Null;
			d.Flags		|= i.Has(DependencyFlag.AutoUpdateAllowed.ToString()) ? DependencyFlag.AutoUpdateAllowed : DependencyFlag.Null;

			return d;
		}

		public void Read(BinaryReader reader)
		{
			Release = reader.Read<ReleaseAddress>();
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
				   EqualityComparer<ReleaseAddress>.Default.Equals(Release, other.Release);
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
		public ReleaseAddress			Release  { get; set; }
		public byte[]					CompleteHash {get; set; }
		public long						CompleteLength {get; set; }
		public Dependency[]				CompleteDependencies {get; set; }
		public IEnumerable<Dependency>	CriticalDependencies => CompleteDependencies.Where(i => i.Type == DependencyType.Critical);

		public byte[]					IncrementalHash {get; set; }
		public long						IncrementalLength {get; set; }
		public Version					IncrementalMinimalVersion {get; set; }
		public Dependency[]				AddedDependencies {get; set; }
		public Dependency[]				RemovedDependencies {get; set; }

 		byte[]							Hash;

		public Manifest()
		{
		}

		public Manifest(byte[]					completehash,
						long					completelength,
						IEnumerable<Dependency>	completecoredependencies,
						byte[]					incrementalhash,
						long					incrementallength,
						Version					incrementalminimalversion,
						IEnumerable<Dependency>	addedcoredependencies,
						IEnumerable<Dependency>	removedcoredependencies)
		{
			CompleteHash				= completehash;
			CompleteLength				= completelength;
			CompleteDependencies		= completecoredependencies?.ToArray() ?? new Dependency[]{};
			
			IncrementalHash				= incrementalhash;
			IncrementalLength			= incrementallength;
			IncrementalMinimalVersion	= incrementalminimalversion;

			AddedDependencies		= addedcoredependencies?.ToArray() ?? new Dependency[]{};
			RemovedDependencies		= removedcoredependencies?.ToArray() ?? new Dependency[]{};
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			if(CompleteHash != null)
			{
				d.Add("CompleteHash").Value = CompleteHash;
				d.Add("CompleteLength").Value = CompleteLength;
	
				if(CompleteDependencies.Any())
				{
					var cd = d.Add("CompleteCoreDependencies");
	
					foreach(var i in CompleteDependencies)
					{
						cd.Add(i.ToString());
					}
				}
			}
	
			if(IncrementalHash != null)
			{
				d.Add("IncrementalHash").Value = IncrementalHash;
				d.Add("IncrementalLength").Value = IncrementalLength;
				d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;

				if(AddedDependencies.Any())
				{
					var cd = d.Add("AddedCoreDependencies");

					foreach(var i in AddedDependencies)
					{
						cd.Add(i.ToString());
					}
				}
	
				if(RemovedDependencies.Any())
				{
					var cd = d.Add("RemovedCoreDependencies");

					foreach(var i in RemovedDependencies)
					{
						cd.Add(i.ToString());
					}
				}
			}

			return d;		
		}

 		public byte[] GetOrCalcHash()
 		{
 			if(Hash != null)
 			{
 				return Hash;
 			}
 
 			var s = new MemoryStream();
 			var w = new BinaryWriter(s);
 	
			Write(w);
				
 			Hash = Cryptography.Current.Hash(s.ToArray());
 		
 			return Hash;
 		}

		public void Write(BinaryWriter w)
		{
			w.Write(CompleteHash != null);

			if(CompleteHash != null)
			{
				w.Write(CompleteHash);
				w.Write7BitEncodedInt64(CompleteLength);
				w.Write(CompleteDependencies);
			}
							
			w.Write(IncrementalHash != null);
				
			if(IncrementalHash != null)
			{
				w.Write(IncrementalHash);
				w.Write7BitEncodedInt64(IncrementalLength);
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
				CompleteLength = r.Read7BitEncodedInt64();
				CompleteDependencies = r.ReadArray<Dependency>();
			}

			if(r.ReadBoolean())
			{
				IncrementalHash = r.ReadSha3();
				IncrementalLength = r.Read7BitEncodedInt64();
				IncrementalMinimalVersion = r.Read<Version>();
				AddedDependencies = r.ReadArray<Dependency>();
				RemovedDependencies = r.ReadArray<Dependency>();
			}
		}
	}
}
