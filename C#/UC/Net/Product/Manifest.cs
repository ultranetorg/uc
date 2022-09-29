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
		public long					CompleteSize;

		public Version				IncrementalMinimalVersion;
		public byte[]				IncrementalHash;
		public long					IncrementalSize;
		
		public ReleaseAddress[]		AddedCoreDependencies;
		public ReleaseAddress[]		RemovedCoreDependencies;

		public byte[]				Hash;
		public bool					Archived;

		public const string			CompleteSizeField = "CompleteSize";
		public const string			CompleteHashField = "CompleteHash";
		public const string			IncrementalSizeField = "IncrementalSize";
		public const string			IncrementalHashField = "IncrementalHash";

		public Manifest()
		{
		}

		public Manifest(ReleaseAddress				address, 
						string						channel, 
						long						completesize,
						byte[]						completehash,
						Version						incrementalminimalversion, 
						long						incrementalsize,
						byte[]						incrementalhash,
						IEnumerable<ReleaseAddress>	addedcoredependencies,
						IEnumerable<ReleaseAddress>	removedcoredependencies)
		{
			Address = address;
			Channel = channel;
			
			CompleteSize				= completesize;
			CompleteHash				= completehash;
			
			IncrementalMinimalVersion	= incrementalminimalversion;
			IncrementalHash				= incrementalhash;
			IncrementalSize				= incrementalsize;

			AddedCoreDependencies		= addedcoredependencies.ToArray();
			RemovedCoreDependencies		= removedcoredependencies.ToArray();
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			d.Add("Address").Value = Address;
			d.Add("Channel").Value = Channel;
	
			if(!Archived)
			{
				d.Add(CompleteHashField).Value = CompleteHash;
				d.Add(CompleteSizeField).Value = CompleteSize;
	
				if(IncrementalSize > 0)
				{
					d.Add("IncrementalMinimalVersion").Value = IncrementalMinimalVersion;
					d.Add(IncrementalHashField).Value = IncrementalHash;
					d.Add(IncrementalSizeField).Value = IncrementalSize;
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

			d.Add("Hash").Value = Hash;

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
			w.Write7BitEncodedInt64(CompleteSize);
			
			w.Write7BitEncodedInt64(IncrementalSize);

			if(IncrementalSize > 0)
			{
				w.Write(IncrementalMinimalVersion);
				w.Write(IncrementalHash);
			}

			w.Write(AddedCoreDependencies);
			w.Write(RemovedCoreDependencies);
				
			Hash = Cryptography.Current.Hash(s.ToArray());
		
			return Hash;
		}

		public void Read(BinaryReader r)
		{
			Address = r.Read<ReleaseAddress>();
			Channel = r.ReadUtf8();

			Archived = r.ReadBoolean();

			if(Archived)
			{
				Hash = r.ReadSha3();
			} 
			else
			{
				CompleteSize = r.Read7BitEncodedInt64();
				CompleteHash = r.ReadSha3();

				IncrementalSize = r.Read7BitEncodedInt64();
				
				if(IncrementalSize > 0)
				{
					IncrementalMinimalVersion = r.ReadVersion();
					IncrementalHash = r.ReadSha3();
				}

				AddedCoreDependencies = r.ReadArray<ReleaseAddress>();
				RemovedCoreDependencies = r.ReadArray<ReleaseAddress>();

				Hash = GetOrCalcHash();
			}
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.WriteUtf8(Channel);

			w.Write(Archived);

			if(Archived)
			{
				w.Write(GetOrCalcHash());
			} 
			else
			{
				w.Write7BitEncodedInt64(CompleteSize);
				w.Write(CompleteHash);
			
				w.Write7BitEncodedInt64(IncrementalSize);

				if(IncrementalSize > 0)
				{
					w.Write(IncrementalMinimalVersion);
					w.Write(IncrementalHash);
				}

				w.Write(AddedCoreDependencies);
				w.Write(RemovedCoreDependencies);
			}
		}
	}
}
