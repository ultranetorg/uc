using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;

namespace Uccs.Net
{
	[Flags]
	public enum ResourceChanges
	{
		Flags			= 0b0000_0001,
		Data			= 0b0000_0010,
		Parent			= 0b0000_0100,
		AnalysisFee		= 0b0000_1000,
		AddPublisher	= 0b0001_0000,
		RemovePublisher	= 0b0010_0000,
	}

	public class ResourceCreation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Initials { get; set; }
		public ResourceFlags		Flags { get; set; }
		public byte[]				Data { get; set; }
		public string				Parent;		
		public Coin					AnalysisFee { get; set; }

		public override bool		Valid => !Flags.HasFlag(ResourceFlags.Child) && (Flags & ResourceFlags.DataMask) != 0;
		public override string		Description => $"{Resource}, {Flags}, {(Data != null ? Hex.ToHexString(Data) : null)}";

		public ResourceCreation()
		{
		}

		public ResourceCreation(AccountAddress signer, ResourceAddress resource, ResourceFlags flags, byte[] data, string parent, Coin analysisfee)
		{
			Signer = signer;
			Resource = resource;
			Flags = flags;

			if(data != null)
			{
				Data = data;
				Initials |= ResourceChanges.Data;
			}

			if(parent != null)
			{
				Parent = parent;
				Initials |= ResourceChanges.Parent;
			}

			if(analysisfee != Coin.Zero)
			{
				AnalysisFee = analysisfee;
				Initials |= ResourceChanges.AnalysisFee;
			}
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Resource	= reader.Read<ResourceAddress>();
			Initials	= (ResourceChanges)reader.ReadByte();
			Flags		= (ResourceFlags)reader.ReadByte();

			if(Initials.HasFlag(ResourceChanges.Data))			Data = reader.ReadBytes();
			if(Initials.HasFlag(ResourceChanges.Parent))		Parent = reader.ReadUtf8();
			if(Initials.HasFlag(ResourceChanges.AnalysisFee))	AnalysisFee = reader.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Initials);
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

if(Resource.Resource == "app/platform/0.0.4")
{
	a=a;
}

			a = round.AffectAuthor(Resource.Author);
			var r = a.AffectResource(Resource);
			
			//a.Resources = a.Resources.Append(r).ToArray();
			
			if(Parent != null)
			{
				r.Flags |= ResourceFlags.Child;
				var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
				p.Resources = p.Resources.Append(r.Id).ToArray();
			}

			r.Flags = r.Flags & (ResourceFlags.Child) | Flags & ~ResourceFlags.Child;
			r.Data = Data;

			if(AnalysisFee > 0)
			{
				round.AffectAccount(Signer).Balance -= AnalysisFee;
	
				r.AnalysisStage = AnalysisStage.Pending;
				r.AnalysisFee = AnalysisFee;
				r.RoundId = round.Id;
				r.AnalysisQuorumRid = 0;
			}

			round.AffectAccount(Signer).Balance -= CalculateSpaceFee(round.Factor, CalculateSize());
		}
	}
}