using System.IO;

namespace Uccs.Net
{
	public enum AnalysisResult : byte
	{
		Null,
		Infected,
		Clean,
		NotEnoughPrepayment,
	}

	public enum AnalysisStage : byte
	{
		NotRequested = 0,  
		Pending,
		HalfVotingReached,
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
		public ResourceAddress	Resource;
		public byte				Good;
		public byte				Bad;
		public bool				Finished =>	(Good >= 0 || Bad >= 0) && !(Good == 255 && Bad == 255);

		public bool	HalfReached
		{
			get => Good == 255 && Bad == 255;
			set
			{
				Good =(byte)255;
				Bad = (byte)255;
			}
		}

		//public bool	NotEnoughPayment
		//{
		//	get => Good == 0 && Bad == 0;
		//	set
		//	{
		//		Good = 0;
		//		Bad = 0;
		//	}
		//}

		public void Read(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Good = reader.ReadByte();
			Bad = reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write(Good);
			writer.Write(Bad);
		}
	}
}
