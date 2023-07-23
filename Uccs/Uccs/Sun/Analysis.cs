using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Net
{
	public enum AnalysisResult : byte
	{
		Infected,
		Clean,
	}

	public enum AnalysisStage : byte
	{
		NotRequested, 
		Pending,
		QuorumReached,
		Finished,
	}

	public class Analysis : IBinarySerializable
	{
		public ResourceAddress		Resource { get; set; }
		public AnalysisResult		Result { get; set; }

		public void Read(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Result	= (AnalysisResult)reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Result);
		}
	}

	public class AnalysisConclusion : IBinarySerializable
	{
		public ResourceAddress	Release;
		public byte				Good;
		public byte				Bad;
		public bool				Finished =>	Good > 0 || Bad > 0;

		public bool	QuorumReached
		{
			get => Good == 255 && Bad == 255;
			set { Good = value ? (byte)255 : (byte)0;
				  Bad = value ? (byte)255 : (byte)0; }
		}


		public void Read(BinaryReader reader)
		{
			Release = reader.Read<ResourceAddress>();
			Good = reader.ReadByte();
			Bad = reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Good);
			writer.Write(Bad);
		}
	}
}
