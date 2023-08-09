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
		Null		= 0, 
		Sealed		= 0b0000_0001, 
		Deprecated	= 0b0000_0010, 
		Child		= 0b0000_0100, 
		Data		= 0b0000_1000, 
		
		NoType		= 0b0000_0000,
		Directory	= 0b0001_0000, 
		Package		= 0b0010_0000, 
		IP4Address	= 0b0011_0000, 
		IP6Address	= 0b0100_0000, 
		Uri			= 0b0101_0000, 
		Redirect	= 0b0110_0000,

		Unchangable	= 0b0000_1100, 
	}

	public class Resource : IBinarySerializable
	{
		public const short		DataLengthMax = 1024;

		public int				Id { get; set; }
		public ResourceAddress	Address { get; set; }
		public ChainTime		Expiration { get; set; }
		public byte				LastRenewalYears { get; set; }
		public ResourceFlags	Flags { get; set; }
		public short			Reserved { get; set; }
		public byte[]			Data { get; set; }
		public AnalysisStage	AnalysisStage { get; set; }
		public Coin				AnalysisFee { get; set; }
		public int				AnalysisHalfVotingRid { get; set; }
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
			return new() {	Id = Id,
							Address	= Address, 
							Expiration = Expiration,
							LastRenewalYears = LastRenewalYears,
							Flags = Flags,
							Reserved = Reserved,
							Data = Data,
							AnalysisStage = AnalysisStage,
							AnalysisFee = AnalysisFee,
							AnalysisHalfVotingRid = AnalysisHalfVotingRid,
							RoundId = RoundId,
							Good = Good,
							Bad = Bad,
							Resources = Resources};
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Id);
			writer.Write((byte)Flags);
			writer.Write(Expiration);
			writer.Write(LastRenewalYears);
			writer.Write(Reserved);
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				writer.WriteBytes(Data);
			}

			writer.Write((byte)AnalysisStage);

			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.HalfVotingReached)
			{
				writer.Write(AnalysisFee);
				writer.Write7BitEncodedInt(RoundId);
				writer.Write7BitEncodedInt(AnalysisHalfVotingRid);
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
			Id					= reader.Read7BitEncodedInt();
			Flags				= (ResourceFlags)reader.ReadByte();
			Expiration			= reader.ReadTime();
			LastRenewalYears	= reader.ReadByte();
			Reserved			= reader.ReadInt16();
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				Data = reader.ReadBytes();
			}

			AnalysisStage = (AnalysisStage)reader.ReadByte();
			
			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.HalfVotingReached)
			{
				AnalysisFee = reader.ReadCoin();
				RoundId = reader.Read7BitEncodedInt();
				AnalysisHalfVotingRid = reader.Read7BitEncodedInt();
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
