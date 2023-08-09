using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceChanges : byte
	{
		Years			= 0b________1,
		Flags			= 0b_______10,
		Data			= 0b______100,
		Parent			= 0b_____1000,
		AnalysisFee		= 0b____10000,
		AddPublisher	= 0b___100000,
		RemovePublisher	= 0b__1000000,
	}

	public class ResourceCreation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Initials { get; set; }
		public ResourceFlags		Flags { get; set; }
		public byte					Years { get; set; }
		public byte[]				Data { get; set; }
		public string				Parent;		
		public Coin					AnalysisFee { get; set; }

		public override bool		Valid => (Flags & ResourceFlags.Unchangable) == 0 && 
											 (!Initials.HasFlag(ResourceChanges.Data) || Initials.HasFlag(ResourceChanges.Data) && Data.Length <= Uccs.Net.Resource.DataLengthMax);
		
		public override string		Description => $"{Resource}, [{Initials}], [{Flags}], Years={Years}{(Parent == null ? null : ", Parent=" + Parent)}{(Data == null ? null : ", Data=" + Hex.ToHexString(Data))}{(AnalysisFee == Coin.Zero ? null : ", AnalysisFee=" + AnalysisFee.ToHumanString())}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(AccountAddress signer, ResourceAddress resource, byte years, ResourceFlags flags, byte[] data, string parent, Coin analysisfee)
		{
			Signer = signer;
			Resource = resource;
			Years = years;
			Flags = flags;

			Initials |= (ResourceChanges.Years|ResourceChanges.Flags);

			if(data != null && data.Length > 0)
			{
				Data = data;
				Initials |= ResourceChanges.Data;
			}

			if(parent != null)
			{
				Parent = parent;
				Initials |= ResourceChanges.Parent;
			}

			if(analysisfee > Coin.Zero)
			{
				AnalysisFee = analysisfee;
				Initials |= ResourceChanges.AnalysisFee;
			}
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Initials	= (ResourceChanges)reader.ReadByte();
			Years		= reader.ReadByte();
			Flags		= (ResourceFlags)reader.ReadByte();

			if(Initials.HasFlag(ResourceChanges.Data))			Data = reader.ReadBytes();
			if(Initials.HasFlag(ResourceChanges.Parent))		Parent = reader.ReadUtf8();
			if(Initials.HasFlag(ResourceChanges.AnalysisFee))	AnalysisFee = reader.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Initials);
			writer.Write(Years);
			writer.Write((byte)Flags);

			if(Initials.HasFlag(ResourceChanges.Data))			writer.WriteBytes(Data);
			if(Initials.HasFlag(ResourceChanges.Parent))		writer.WriteUtf8(Parent);
			if(Initials.HasFlag(ResourceChanges.AnalysisFee))	writer.Write(AnalysisFee);
		}

		public override void Execute(Chainbase chain, Round round)
		{
			var a = chain.Authors.Find(Resource.Author, round.Id);

			if(a == null)
			{
				Error = NotFound;
				return;
			}

			if(a.Owner != Signer)
			{
				Error = NotOwnerOfAuthor;
				return;
			}

			var e = chain.Authors.FindResource(Resource, round.Id);
					
			if(e != null)
			{
				Error = AlreadyExists;
				return;
			}

			a = round.AffectAuthor(Resource.Author);
			var r = a.AffectResource(Resource);

			r.Flags = r.Flags & ResourceFlags.Unchangable | Flags & ~ResourceFlags.Unchangable;

			r.Expiration = round.ConfirmedTime + ChainTime.FromYears(Years);
			round.AffectAccount(Signer).Balance -= CalculateSpaceFee(round.Factor, CalculateSize(), Years);
			
			if(Parent != null)
			{
				r.Flags |= ResourceFlags.Child;
				var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
				p.Resources = p.Resources.Append(r.Id).ToArray();
			}
						
			if(Data != null)
			{
				r.Flags |= ResourceFlags.Data;
				r.Reserved	= (short)Data.Length;
				r.Data		= Data;
			}

			if(AnalysisFee > 0)
			{
				round.AffectAccount(Signer).Balance -= AnalysisFee;
	
				r.AnalysisStage			= AnalysisStage.Pending;
				r.AnalysisFee			= AnalysisFee;
				r.RoundId				= round.Id;
				r.AnalysisHalfVotingRid = 0;
			}
		}
	}
}