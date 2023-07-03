using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	public class Release : IBinarySerializable
	{
		public ReleaseAddress	Address { get; set; }
		public byte[]			Manifest { get; set; }
		public string			Channel { get; set; } /// stable, beta, nightly, debug,...
		public AnalysisStage	AnalysisStage { get; set; }
		public Coin				AnalysisFee { get; set; }
		public int				RoundId { get; set; }
		public int				QuorumRid { get; set; }
		public byte				Good { get; set; }
		public byte				Bad { get; set; }
		
		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write(Manifest);
			w.WriteUtf8(Channel);
			w.Write((byte)AnalysisStage);

			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				w.Write(AnalysisFee);
				w.Write7BitEncodedInt(RoundId);
				w.Write7BitEncodedInt(QuorumRid);
			}
			if(AnalysisStage == AnalysisStage.Finished)
			{
				w.Write(Good);
				w.Write(Bad);
			}
		}

		public void Read(BinaryReader r)
		{
			Address	= r.Read<ReleaseAddress>();
			Manifest = r.ReadSha3();
			Channel = r.ReadUtf8();
			AnalysisStage = (AnalysisStage)r.ReadByte();
			
			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				AnalysisFee = r.ReadCoin();
				RoundId = r.Read7BitEncodedInt();
				QuorumRid = r.Read7BitEncodedInt();
			}
			if(AnalysisStage == AnalysisStage.Finished)
			{
				Good = r.ReadByte();
				Bad = r.ReadByte();	
			}
		}
	}
}
