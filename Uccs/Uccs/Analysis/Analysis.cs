using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{
	public enum AnalysisResult : byte
	{
		None,
		Negative,
		Positive,
		NotEnoughPrepayment,
	}

	//public enum AnalysisStage : byte
	//{
	//	NotRequested = 0,  
	//	Pending,
	//	HalfVotingReached,
	//	Finished,
	//}

	public struct AnalyzerResult
	{
		public byte				Analyzer { get; set; }
		public AnalysisResult	Result { get; set; }

		public override string ToString()
		{
			return $"{Analyzer}={Result}";
		}
	}

	public class Analysis : IBinarySerializable
	{
		public byte[]			Release { get; set; }
		public Money			Fee { get; set; }
		public int				StartedAt { get; set; }
		public byte				Consil { get; set; }
		public AnalyzerResult[]	Results { get; set; }

		public void Read(BinaryReader reader)
		{
			Release		= reader.ReadHash();
			Fee			= reader.ReadMoney();
			StartedAt	= reader.Read7BitEncodedInt();
			Consil		= reader.ReadByte();
			Results		= reader.ReadArray(() => new AnalyzerResult {Analyzer = reader.ReadByte(), Result = (AnalysisResult)reader.ReadByte()});
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Release);
			writer.Write(Fee);
			writer.Write7BitEncodedInt(StartedAt);
			writer.Write(Consil);
			writer.Write(Results, i => { writer.Write(i.Analyzer); writer.Write((byte)i.Result); });
		}
	}
}
