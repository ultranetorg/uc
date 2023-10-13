using System;
using System.IO;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceFlags : byte
	{
		None		= 0, 
		Sealed		= 0b________1, 
		Deprecated	= 0b_______10, 
		Child		= 0b______100, 
		Data		= 0b_____1000, 

		Unchangables= Child | Data, 
	}

	public enum ResourceType : short
	{
		None		= 0,
		Redirect	= 1,
		IP4Address	= 2, 
		IP6Address	= 3, 
		Uri			= 4,
				
		File		= 100, 
		Directory	= 101, 
		Package		= 102, 

		FirstMime	= 1000, 
	}

	[Flags]
	public enum ResourceChanges : ushort
	{
		None			= 0,
		Years			= 0b_______________1,
		Flags			= 0b______________10,
		Type			= 0b_____________100,
		Data			= 0b____________1000,
		Parent			= 0b___________10000,
		AnalysisFee		= 0b__________100000,
		AddPublisher	= 0b_________1000000,
		RemovePublisher	= 0b________10000000,
		Recursive		= 0b1000000000000000,
	}

	public class Resource : IBinarySerializable
	{
		public const short		DataLengthMax = 1024;

		public int				Id { get; set; }
		public ResourceAddress	Address { get; set; }
		public Time				Expiration { get; set; }
		public byte				LastRenewalYears { get; set; }
		public ResourceFlags	Flags { get; set; }
		public ResourceType		Type { get; set; }
		public short			Reserved { get; set; }
		public byte[]			Data { get; set; }
		public AnalysisStage	AnalysisStage { get; set; }
		public Money			AnalysisFee { get; set; }
		public int				AnalysisHalfVotingRound { get; set; }
		public int				RoundId { get; set; }
		public byte				Good { get; set; }
		public byte				Bad { get; set; }
		public int[]			Resources { get; set; } = {};

		public Time				RenewalBegin => Expiration - Time.FromYears(1);

		public override string ToString()
		{
			return $"{Id}, {Address}, {Expiration}, {LastRenewalYears}, [{Flags}], {Type}, Reserved={Reserved}, Data={(Data == null ? null : ('[' + Data.Length + ']'))} Resources={{{Resources.Length}}}";
		}

		public Resource Clone()
		{
			return new() {	Id = Id,
							Address	= Address, 
							Expiration = Expiration,
							LastRenewalYears = LastRenewalYears,
							Flags = Flags,
							Type = Type,
							Reserved = Reserved,
							Data = Data,
							AnalysisStage = AnalysisStage,
							AnalysisFee = AnalysisFee,
							AnalysisHalfVotingRound = AnalysisHalfVotingRound,
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
			writer.Write7BitEncodedInt((int)Type);
			writer.Write7BitEncodedInt(Reserved);
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				writer.WriteBytes(Data);
			}

			writer.Write((byte)AnalysisStage);

			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.HalfVotingReached)
			{
				writer.Write(AnalysisFee);
				writer.Write7BitEncodedInt(RoundId);
				writer.Write7BitEncodedInt(AnalysisHalfVotingRound);
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
			Type				= (ResourceType)reader.Read7BitEncodedInt();
			Reserved			= (short)reader.Read7BitEncodedInt();
			
			if(Flags.HasFlag(ResourceFlags.Data))
			{
				Data = reader.ReadBytes();
			}

			AnalysisStage = (AnalysisStage)reader.ReadByte();
			
			if(AnalysisStage == AnalysisStage.Pending || AnalysisStage == AnalysisStage.HalfVotingReached)
			{
				AnalysisFee = reader.ReadCoin();
				RoundId = reader.Read7BitEncodedInt();
				AnalysisHalfVotingRound = reader.Read7BitEncodedInt();
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
