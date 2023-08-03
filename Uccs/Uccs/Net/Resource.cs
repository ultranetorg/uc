using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		Null, 
		Variable	= 0b0_0_000_00, 
		Constant	= 0b0_0_000_01, 

		Data		= 0b0_0_000_00,
		File		= 0b0_0_001_00, 
		FileTree	= 0b0_0_010_00, 
		Package		= 0b0_0_011_00, 
		IP4Address	= 0b0_0_100_00, 
		IP6Address	= 0b0_0_101_00, 
		Uri			= 0b0_0_110_00, 
		Redirect	= 0b0_0_111_00,
	
		Analysable	= 0b0_1_000_00, 

		Deprecated	= 0b1_0_000_00, 
	}

	public class Resource : IBinarySerializable
	{
		public ResourceAddress	Address { get; set; }
		public byte[]			Data { get; set; }
		public ResourceFlags	Flags { get; set; }
		public int				RoundId { get; set; }
		public AnalysisStage	AnalysisStage { get; set; }
		public Coin				AnalysisFee { get; set; }
		public int				AnalysisQuorumRid { get; set; }
		public byte				Good { get; set; }
		public byte				Bad { get; set; }

		public void Write(BinaryWriter w)
		{
			w.Write(Address);
			w.Write((byte)Flags);
			w.WriteBytes(Data);
			w.Write((byte)AnalysisStage);

			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				w.Write(AnalysisFee);
				w.Write7BitEncodedInt(RoundId);
				w.Write7BitEncodedInt(AnalysisQuorumRid);
			}
			if(AnalysisStage == AnalysisStage.Finished)
			{
				w.Write(Good);
				w.Write(Bad);
			}
		}

		public void Read(BinaryReader r)
		{
			Address			= r.Read<ResourceAddress>();
			Flags			= (ResourceFlags)r.ReadByte();
			Data			= r.ReadBytes();
			AnalysisStage	= (AnalysisStage)r.ReadByte();
			
			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				AnalysisFee = r.ReadCoin();
				RoundId = r.Read7BitEncodedInt();
				AnalysisQuorumRid = r.Read7BitEncodedInt();
			}
			if(AnalysisStage == AnalysisStage.Finished)
			{
				Good = r.ReadByte();
				Bad = r.ReadByte();	
			}
		}
	}
}
