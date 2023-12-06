using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;

namespace Uccs.Net
{
	[Flags]
	public enum PackageReleaseFlag
	{
		None, 
		Complete	= 0b01,
		Incremental = 0b10,
	}

	public class HistoryRelease : IBinarySerializable
	{
		public byte[]				Hash { get; set; }
		public PackageReleaseFlag	Flags { get; set; }
		public int					IncrementalMinimal { get; set; }

		public void Write(BinaryWriter writer)
		{
			writer.WriteBytes(Hash);
			writer.Write((byte)Flags);
			
			if(Flags.HasFlag(PackageReleaseFlag.Incremental))
				writer.Write7BitEncodedInt(IncrementalMinimal);

			//writer.Write(Version);
			//writer.WriteBytes(Complete);
			//writer.WriteBytes(Incremental);
			//
			//if(Incremental != null)
			//{
			//	writer.Write(IncrementalMinimalVersion);
			//}
		}

		public void Read(BinaryReader reader)
		{
			Hash = reader.ReadBytes();
			Flags = (PackageReleaseFlag)reader.ReadByte();
			
			if(Flags.HasFlag(PackageReleaseFlag.Incremental))
				IncrementalMinimal = reader.Read7BitEncodedInt();
			
			///Version = reader.Read<Version>();
			///Complete = reader.ReadBytes();
			///Incremental = reader.ReadBytes();
			///
			///if(Incremental != null)
			///{
			///	IncrementalMinimalVersion = reader.Read<Version>();
			///}
		}
	}

	public class History : IBinarySerializable
	{
		public ResourceAddress			Address { get; set; }
		public List<HistoryRelease>		Releases { get; set; }

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

		public History()
		{
		}

		public History(byte[] data)
		{
			Read(new BinaryReader(new MemoryStream(data)));
		}

		public void Write(BinaryWriter w)
		{
			w.Write(Releases);
		}

		public void Read(BinaryReader r)
		{
			Releases = r.ReadList<HistoryRelease>();
		}

		public XonDocument ToXon(IXonValueSerializator serializator)
		{
			var d = new XonDocument(serializator);

			foreach(var i in Releases)
			{
				var n = d.Add(i.Hash.ToHex());
				n.Value = i.Flags.ToString();

				if(i.Flags.HasFlag(PackageReleaseFlag.Incremental))
				{
					n.Add("IncrementalMinimal").Value = i.IncrementalMinimal;
				}
			}

			return d;		
		}
	}
}
