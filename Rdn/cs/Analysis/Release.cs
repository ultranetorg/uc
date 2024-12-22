namespace Uccs.Rdn;

[Flags]
public enum ReleaseFlag : byte
{
	None,
	Analysis = 0b_______1
}


//public enum AnalysisStage : byte
//{
//	NotRequested = 0,  
//	Pending,
//	HalfVotingReached,
//	Finished,
//}

// 	public class Release : IBinarySerializable
// 	{
// 		public ReleaseAddress		Address { get; set; }
// 		public ReleaseFlag			Flags { get; set; }
// 
// 		public void Read(BinaryReader reader)
// 		{
// 			Address		= reader.Read<ReleaseAddress>(ReleaseAddress.FromType);
// 			Flags		= (ReleaseFlag)reader.ReadByte();
// 			Expiration	= reader.Read<Time>();
// 			
// 			if(Flags.HasFlag(ReleaseFlag.Analysis))
// 			{
// 				Fee			= reader.Read<Money>();
// 				StartedAt	= reader.Read<Time>();
// 				Consil		= reader.ReadByte();
// 				Results		= reader.ReadArray(() => new AnalyzerResult{AnalyzerId = reader.ReadByte(), 
// 																		Result = (AnalysisResult)reader.ReadByte()});
// 			}
// 		}
// 
// 		public void Write(BinaryWriter writer)
// 		{
// 			writer.Write(Address);
// 			writer.Write((byte)Flags);
// 			writer.Write(Expiration);
// 
// 			if(Flags.HasFlag(ReleaseFlag.Analysis))
// 			{
// 				writer.Write(Fee);
// 				writer.Write(StartedAt);
// 				writer.Write(Consil);
// 				writer.Write(Results, i => { writer.Write(i.AnalyzerId);
// 											 writer.Write((byte)i.Result); });
// 			}
// 		}
//	}
