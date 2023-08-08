using System;
using System.IO;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;

namespace Uccs.Net
{
	public class ResourceUpdation : Operation
	{
		public ResourceAddress		Resource { get; set; }
		public ResourceChanges		Changes	{ get; set; }
		public ResourceFlags		Flags { get; set; }
		public byte[]				Data { get; set; }
		public string				Parent;		
		public Coin					AnalysisFee { get; set; }

		public override bool		Valid => !Flags.HasFlag(ResourceFlags.Child);
		public override string		Description => $"{Resource}, {Flags}, {(Data != null ? Hex.ToHexString(Data) : null)}";

		public ResourceUpdation()
		{
		}

		public ResourceUpdation(AccountAddress signer, ResourceAddress resource)
		{
			Signer = signer;
			Resource = resource;
		}

		public void Change(ResourceFlags flags)
		{
			Flags = flags;
			Changes |= ResourceChanges.Flags;
		}

		public void Change(byte[] data)
		{
			Data = data;
			Changes |= ResourceChanges.Data;
		}

		public void Change(string parent)
		{
			Parent = parent;
			Changes |= ResourceChanges.Parent;
		}

		public void Change(Coin analysisfee)
		{
			AnalysisFee = analysisfee;
			Changes |= ResourceChanges.AnalysisFee;
		}

		protected override void ReadConfirmed(BinaryReader reader)
		{
			Resource = reader.Read<ResourceAddress>();
			Changes = (ResourceChanges)reader.ReadByte();
			
			if(Changes.HasFlag(ResourceChanges.Flags))			Flags = (ResourceFlags)reader.ReadByte();
			if(Changes.HasFlag(ResourceChanges.Data))			Data = reader.ReadBytes();
			if(Changes.HasFlag(ResourceChanges.Parent))			Parent = reader.ReadUtf8();
			if(Changes.HasFlag(ResourceChanges.AnalysisFee))	AnalysisFee = reader.ReadCoin();
		}

		protected override void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Resource);
			writer.Write((byte)Changes);

			if(Changes.HasFlag(ResourceChanges.Flags))			writer.Write((byte)Flags);
			if(Changes.HasFlag(ResourceChanges.Data))			writer.WriteBytes(Data);
			if(Changes.HasFlag(ResourceChanges.Parent))			writer.WriteUtf8(Parent);
			if(Changes.HasFlag(ResourceChanges.AnalysisFee))	writer.Write(AnalysisFee);
		}

		public override void Execute(Chainbase chain, Round round)
		{
			var e = chain.Authors.FindResource(Resource, round.Id);
			
			if(e == null) 
			{
				Error = NotFound;
				return;
			}

			if(Changes.HasFlag(ResourceChanges.Data) && e.Flags.HasFlag(ResourceFlags.Sealed))
			{
				Error = CantChangeSealedResource;
				return;
			}

			var a = round.AffectAuthor(Resource.Author);
			var r = a.AffectResource(Resource);

			if(Changes.HasFlag(ResourceChanges.Parent))
			{
				if(e.Flags.HasFlag(ResourceFlags.Child)) /// remove from existing parent
				{
					var p = a.Resources.First(i => i.Resources.Contains(e.Id));
					p = a.AffectResource(p.Address);
					p.Resources = p.Resources.Where(i => i != e.Id).ToArray();
				} 
								
				if(Parent == null)
				{
					r.Flags &= ~ResourceFlags.Child;
				}
				else
				{
					r.Flags |= ResourceFlags.Child;
					var p = a.AffectResource(new ResourceAddress(a.Name, Parent));
					p.Resources = p.Resources.Append(r.Id).ToArray();
				}
			}

			if(Changes.HasFlag(ResourceChanges.Flags))
			{
				r.Flags = r.Flags & ResourceFlags.Child | Flags & ~ResourceFlags.Child;
			}

			if(Changes.HasFlag(ResourceChanges.Data))
			{
				r.Data = Data;

				round.AffectAccount(Signer).Balance -= CalculateSpaceFee(round.Factor, (Data != null ? Data.Length : 0) - (r.Data != null ? r.Data.Length : 0));
			}

			if(Changes.HasFlag(ResourceChanges.AnalysisFee))
			{
				if(AnalysisFee > 0 && r.AnalysisStage == AnalysisStage.NotRequested)
				{
					round.AffectAccount(Signer).Balance -= AnalysisFee;
	
					r.AnalysisStage = AnalysisStage.Pending;
					r.AnalysisFee = AnalysisFee;
					r.RoundId = round.Id;
					r.AnalysisQuorumRid = 0;
				}
			}
		}
	}
}