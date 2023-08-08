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
		Sealed		= 0b0_0000_001, 
		Deprecated	= 0b0_0000_010, 
		Child		= 0b0_0000_100, 
		
		DataMask	= 0b0_1111_000,
		Empty		= 0b0_0001_000,
		Data		= 0b0_0010_000,
		Directory	= 0b0_0011_000, 
		Package		= 0b0_0100_000, 
		IP4Address	= 0b0_0101_000, 
		IP6Address	= 0b0_0110_000, 
		Uri			= 0b0_0111_000, 
		Redirect	= 0b0_1000_000,
						
		//Analysable	= 0b1_0000_000, 
	}

	public class Resource : IBinarySerializable
	{
		public int				Id { get; set; }
		public ResourceAddress	Address { get; set; }
		public ResourceFlags	Flags { get; set; }
		public byte[]			Data { get; set; }
		public AnalysisStage	AnalysisStage { get; set; }
		public Coin				AnalysisFee { get; set; }
		public int				AnalysisQuorumRid { get; set; }
		public int				RoundId { get; set; }
		public byte				Good { get; set; }
		public byte				Bad { get; set; }
		public int[]			Resources { get; set; } = {};

		public override string ToString()
		{
			return $"{Id}, {Address}, [{Flags}], Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Address	= Address, 
							Id = Id,
							Flags = Flags,
							Data = Data,
							AnalysisStage = AnalysisStage,
							AnalysisFee = AnalysisFee,
							AnalysisQuorumRid = AnalysisQuorumRid,
							RoundId = RoundId,
							Good = Good,
							Bad = Bad,
							Resources = Resources};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Id);
			writer.Write((byte)Flags);
			
			if(!Flags.HasFlag(ResourceFlags.Empty))
			{
				writer.WriteBytes(Data);
			}

			writer.Write((byte)AnalysisStage);

			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				writer.Write(AnalysisFee);
				writer.Write7BitEncodedInt(RoundId);
				writer.Write7BitEncodedInt(AnalysisQuorumRid);
			}

			if(AnalysisStage == AnalysisStage.Finished)
			{
				writer.Write(Good);
				writer.Write(Bad);
			}

			writer.Write(Resources, i => writer.Write7BitEncodedInt(i));
		}

		public void Read(BinaryReader reader)
		{
			Id		= reader.Read7BitEncodedInt();
			Flags	= (ResourceFlags)reader.ReadByte();
			
			if(!Flags.HasFlag(ResourceFlags.Empty))
			{
				Data = reader.ReadBytes();
			}

			AnalysisStage = (AnalysisStage)reader.ReadByte();
			
			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.QuorumReached)
			{
				AnalysisFee = reader.ReadCoin();
				RoundId = reader.Read7BitEncodedInt();
				AnalysisQuorumRid = reader.Read7BitEncodedInt();
			}

			if(AnalysisStage == AnalysisStage.Finished)
			{
				Good = reader.ReadByte();
				Bad = reader.ReadByte();	
			}

			Resources = reader.ReadArray(() => reader.Read7BitEncodedInt());
		}
	}
}
