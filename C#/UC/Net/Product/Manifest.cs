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
		public ReleaseAddress		Address;
		public string				Channel;		/// stable, beta, nightly, debug,...
		public byte[]				CompleteHash;

		public Version				IncrementalMinimalVersion;
		public byte[]				IncrementalHash;
		
		public ReleaseAddress[]		AddedCoreDependencies;
		public ReleaseAddress[]		RemovedCoreDependencies;

 		byte[]						Hash;
// 		public bool					Archived;

		public const string			CompleteHashField = "CompleteHash";
		public const string			IncrementalHashField = "IncrementalHash";

		public Manifest()
		{
		}

		public Manifest(ReleaseAddress				address,
						string						channel,
						byte[]						completehash,
						Version						incrementalminimalversion,
						byte[]						incrementalhash,
						IEnumerable<ReleaseAddress>	addedcoredependencies,
						IEnumerable<ReleaseAddress>	removedcoredependencies)
		{
			Address						= address;
			Channel						= channel;
			
			CompleteHash				= completehash;
			
			IncrementalMinimalVersion	= incrementalminimalversion;
			IncrementalHash				= incrementalhash;

			AddedCoreDependencies		= addedcoredependencies?.ToArray() ?? new ReleaseAddress[]{};
			RemovedCoreDependencies		= removedcoredependencies?.ToArray() ?? new ReleaseAddress[]{};
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			d.Add("Address").Value = Address;
			d.Add("Channel").Value = Channel;
	
			//if(!Archived)
			{
				d.Add(CompleteHashField).Value = CompleteHash;
	
				if(IncrementalHash != null)
				{
					d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;
					d.Add(IncrementalHashField).Value = IncrementalHash;
				}
	
				if(AddedCoreDependencies.Any())
				{
					var cd = d.Add("AddedCoreDependencies");

					foreach(var i in AddedCoreDependencies)
					{
						cd.Add(i.ToString());
					}
				}
	
				if(RemovedCoreDependencies.Any())
				{
					var cd = d.Add("RemovedCoreDependencies");

					foreach(var i in RemovedCoreDependencies)
					{
						cd.Add(i.ToString());
					}
				}
			}

			//d.Add("Hash").Value = Hash;

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
 	
 			w.Write(Address);
 			w.WriteUtf8(Channel);
 			w.Write(CompleteHash);
 
 			if(IncrementalHash != null)
 			{
 				w.Write(IncrementalMinimalVersion);
 				w.Write(IncrementalHash);
 			}
 
 			w.Write(AddedCoreDependencies);
 			w.Write(RemovedCoreDependencies);
 				
 			Hash = Cryptography.Current.Hash(s.ToArray());
 		
 			return Hash;
 		}

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Channel);

			//w.Write(Archived);

			//if(Archived)
			//{
			//	w.Write(GetOrCalcHash());
			//} 
			//else
			{
				w.Write(CompleteHash != null);

				if(CompleteHash != null)
					w.Write(CompleteHash);
			
				w.Write(IncrementalHash != null);
				
				if(IncrementalHash != null)
				{
					w.Write(IncrementalMinimalVersion);
					w.Write(IncrementalHash);
				}

				w.Write(AddedCoreDependencies);
				w.Write(RemovedCoreDependencies);
			}
		}

		public void Read(BinaryReader r)
		{
			Address = r.Read<ReleaseAddress>();
			Channel = r.ReadUtf8();

			//Archived = r.ReadBoolean();

			//if(Archived)
			//{
			//	Hash = r.ReadSha3();
			//} 
			//else
			{
				if(r.ReadBoolean())
					CompleteHash = r.ReadSha3();

				if(r.ReadBoolean())
				{
					IncrementalMinimalVersion = r.Read<Version>();
					IncrementalHash = r.ReadSha3();
				}

				AddedCoreDependencies = r.ReadArray<ReleaseAddress>();
				RemovedCoreDependencies = r.ReadArray<ReleaseAddress>();

				//Hash = GetOrCalcHash();
			}
		}
	}
}
