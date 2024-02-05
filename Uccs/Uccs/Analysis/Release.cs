using System.Collections.Generic;
using System.IO;

namespace Uccs.Net
{
	public enum AnalysisResult : byte
	{
		None,
		Negative,
		Positive,
		Vulnerable,
		//NotEnoughPrepayment,
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
		public byte				AnalyzerId { get; set; }
		public AnalysisResult	Result { get; set; }

		public override string ToString()
		{
			return $"{AnalyzerId}={Result}";
		}
	}

	public class Release : IBinarySerializable
	{
		public ReleaseAddress	Address { get; set; }
		public Money			Fee { get; set; }
		public int				StartedAt { get; set; }
		public byte				Consil { get; set; }
		public AnalyzerResult[]	Results { get; set; }

		public void Read(BinaryReader reader)
		{
			Address		= reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
			Fee			= reader.ReadMoney();
			StartedAt	= reader.Read7BitEncodedInt();
			Consil		= reader.ReadByte();
			Results		= reader.ReadArray(() => new AnalyzerResult {AnalyzerId = reader.ReadByte(), Result = (AnalysisResult)reader.ReadByte()});
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Address);
			writer.Write(Fee);
			writer.Write7BitEncodedInt(StartedAt);
			writer.Write(Consil);
			writer.Write(Results, i => { writer.Write(i.AnalyzerId); writer.Write((byte)i.Result); });
		}
	}
}
