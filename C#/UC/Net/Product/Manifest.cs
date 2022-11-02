using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UC.Net
{
	public class Manifest : IBinarySerializable
	{
		public ReleaseAddress		Release;
		//public string				Channel;		/// stable, beta, nightly, debug,...
		public byte[]				CompleteHash;
		public long					CompleteLength;
		public ReleaseAddress[]		CompleteDependencies;

		public byte[]				IncrementalHash;
		public long					IncrementalLength;
		public Version				IncrementalMinimalVersion;
		public ReleaseAddress[]		AddedDependencies;
		public ReleaseAddress[]		RemovedDependencies;

 		byte[]						Hash;
// 		public bool					Archived;

		public Manifest()
		{
		}

		public Manifest(byte[]						completehash,
						long						completelength,
						IEnumerable<ReleaseAddress>	completecoredependencies,
						byte[]						incrementalhash,
						long						incrementallength,
						Version						incrementalminimalversion,
						IEnumerable<ReleaseAddress>	addedcoredependencies,
						IEnumerable<ReleaseAddress>	removedcoredependencies)
		{
			CompleteHash				= completehash;
			CompleteLength				= completelength;
			CompleteDependencies		= completecoredependencies?.ToArray() ?? new ReleaseAddress[]{};
			
			IncrementalHash				= incrementalhash;
			IncrementalLength			= incrementallength;
			IncrementalMinimalVersion	= incrementalminimalversion;

			AddedDependencies		= addedcoredependencies?.ToArray() ?? new ReleaseAddress[]{};
			RemovedDependencies		= removedcoredependencies?.ToArray() ?? new ReleaseAddress[]{};
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
				CompleteDependencies = r.ReadArray<ReleaseAddress>();
			}

			if(r.ReadBoolean())
			{
				IncrementalHash = r.ReadSha3();
				IncrementalLength = r.Read7BitEncodedInt64();
				IncrementalMinimalVersion = r.Read<Version>();
				AddedDependencies = r.ReadArray<ReleaseAddress>();
				RemovedDependencies = r.ReadArray<ReleaseAddress>();
			}
		}
	}
}
